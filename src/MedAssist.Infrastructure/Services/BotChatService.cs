using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MedAssist.Application.DTOs;
using MedAssist.Application.Exceptions;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Entities;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedAssist.Infrastructure.Services;

public class BotChatService : IBotChatService
{
    private const string SystemPrompt = """
                                        Ты помощник для врача в Telegram.
                                        Отвечай только на русском языке.
                                        Не ставь окончательных диагнозов, отмечай ограничения дистанционного формата.
                                        При признаках неотложных состояний явно советуй срочно обратиться за очной медицинской помощью.
                                        Отвечай структурированно и кратко.
                                        """;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly MedAssistDbContext _db;
    private readonly ISystemSettingsService _systemSettingsService;
    private readonly ILogger<BotChatService> _logger;

    public BotChatService(
        MedAssistDbContext db,
        ISystemSettingsService systemSettingsService,
        ILogger<BotChatService> logger)
    {
        _db = db;
        _systemSettingsService = systemSettingsService;
        _logger = logger;
    }

    public async Task<BotChatAnswerDto> AskAsync(AskBotQuestionRequest request, CancellationToken cancellationToken)
    {
        var existingTurn = await _db.BotChatTurns
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RequestId == request.RequestId, cancellationToken);
        if (existingTurn is not null)
        {
            if (request.ConversationId.HasValue && existingTurn.ConversationId != request.ConversationId.Value)
            {
                throw new ConflictException("requestId is already used in another conversation.");
            }

            return ToDto(existingTurn);
        }

        var now = DateTimeOffset.UtcNow;
        var (conversation, isNewConversation) = await ResolveConversationAsync(request, now, cancellationToken);

        var doctor = await _db.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TelegramUserId == request.TelegramUserId, cancellationToken);
        if (doctor is null)
        {
            throw new KeyNotFoundException("Doctor not found for provided telegramUserId.");
        }
        if (doctor.TokenBalance <= 0)
        {
            throw new ConflictException("Insufficient token balance.");
        }

        Patient? activePatient = null;
        if (doctor.LastSelectedPatientId.HasValue)
        {
            activePatient = await _db.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.Id == doctor.LastSelectedPatientId.Value && p.DoctorId == doctor.Id,
                    cancellationToken);
        }

        var history = await _db.BotChatTurns
            .AsNoTracking()
            .Where(x => x.ConversationId == conversation.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);
        history.Reverse();

        var prompt = BuildPrompt(doctor, activePatient, history, request.Text);
        var llmResult = await GenerateAsync(prompt, cancellationToken);

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Повторная проверка внутри транзакции, чтобы не списать токены на дубликате.
            var duplicatedInTx = await _db.BotChatTurns
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RequestId == request.RequestId, cancellationToken);
            if (duplicatedInTx is not null)
            {
                await tx.RollbackAsync(cancellationToken);
                return ToDto(duplicatedInTx);
            }

            var charged = await _db.Doctors
                .Where(x => x.Id == doctor.Id && x.TokenBalance > 0)
                .ExecuteUpdateAsync(
                    updates => updates.SetProperty(d => d.TokenBalance, d => d.TokenBalance - 1),
                    cancellationToken);
            if (charged == 0)
            {
                await tx.RollbackAsync(cancellationToken);
                throw new ConflictException("Insufficient token balance.");
            }
            var balanceAfter = await _db.Doctors
                .Where(x => x.Id == doctor.Id)
                .Select(x => x.TokenBalance)
                .SingleAsync(cancellationToken);

            if (isNewConversation)
            {
                _db.BotConversations.Add(conversation);
            }
            else
            {
                conversation.UpdatedAt = now;
            }

            var turn = new BotChatTurn
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                RequestId = request.RequestId,
                UserText = request.Text.Trim(),
                AssistantText = llmResult.Content.Trim(),
                Provider = llmResult.Provider,
                Model = llmResult.Model,
                PromptTokens = llmResult.PromptTokens,
                CompletionTokens = llmResult.CompletionTokens,
                ProviderRequestId = llmResult.RequestId,
                CreatedAt = now
            };

            _db.BotChatTurns.Add(turn);
            _db.BillingTokenLedgers.Add(new Domain.Entities.BillingTokenLedger
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                TelegramUserId = request.TelegramUserId,
                Delta = -1,
                BalanceAfter = balanceAfter,
                Reason = "ChatAskSuccess",
                ConversationId = conversation.Id,
                ChatTurnId = turn.Id,
                RequestId = request.RequestId,
                CreatedAt = now
            });
            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
            return ToDto(turn);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            _logger.LogWarning(ex, "Bot chat save failed, trying idempotent fallback by requestId={RequestId}", request.RequestId);
            var duplicated = await _db.BotChatTurns
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RequestId == request.RequestId, cancellationToken);
            if (duplicated is not null)
            {
                return ToDto(duplicated);
            }

            throw;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<BotConversationHistoryDto>> GetConversationsAsync(
        long? telegramUserId,
        int take,
        CancellationToken cancellationToken)
    {
        var normalizedTake = Math.Clamp(take, 1, 200);
        var query = _db.BotConversations
            .AsNoTracking()
            .AsQueryable();

        if (telegramUserId.HasValue)
        {
            query = query.Where(x => x.TelegramUserId == telegramUserId.Value);
        }

        var conversations = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Take(normalizedTake)
            .Select(x => new BotConversationHistoryDto(
                x.Id,
                x.TelegramUserId,
                x.Turns.Count,
                x.Turns
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => t.UserText)
                    .FirstOrDefault(),
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return conversations.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<BotChatTurnHistoryDto>> GetTurnsAsync(
        Guid conversationId,
        int take,
        CancellationToken cancellationToken)
    {
        var normalizedTake = Math.Clamp(take, 1, 500);
        var turns = await _db.BotChatTurns
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedTake)
            .Select(x => new BotChatTurnHistoryDto(
                x.Id,
                x.ConversationId,
                x.RequestId,
                x.UserText,
                x.AssistantText,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return turns.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<BotChatTurnHistoryDto>> GetTurnsAsync(
        long telegramUserId,
        Guid conversationId,
        int take,
        CancellationToken cancellationToken)
    {
        var exists = await _db.BotConversations
            .AsNoTracking()
            .AnyAsync(x => x.Id == conversationId && x.TelegramUserId == telegramUserId, cancellationToken);
        if (!exists)
        {
            throw new KeyNotFoundException("Conversation not found.");
        }

        return await GetTurnsAsync(conversationId, take, cancellationToken);
    }

    private async Task<(BotConversation Conversation, bool IsNew)> ResolveConversationAsync(
        AskBotQuestionRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (!request.ConversationId.HasValue)
        {
            return (new BotConversation
            {
                Id = Guid.NewGuid(),
                TelegramUserId = request.TelegramUserId,
                CreatedAt = now,
                UpdatedAt = now
            }, true);
        }

        var existing = await _db.BotConversations
            .FirstOrDefaultAsync(
                x => x.Id == request.ConversationId.Value && x.TelegramUserId == request.TelegramUserId,
                cancellationToken);

        if (existing is null)
        {
            throw new KeyNotFoundException("Conversation not found.");
        }

        return (existing, false);
    }

    private async Task<LlmGatewayGenerateResponse> GenerateAsync(string prompt, CancellationToken cancellationToken)
    {
        var baseUrl = await _systemSettingsService.GetLlmGatewayUrlAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("LLM Gateway URL is not configured in system settings.");
        }

        var endpoint = $"{baseUrl.TrimEnd('/')}/v1/generate";
        var payload = new LlmGatewayGenerateRequest
        {
            Prompt = prompt,
            Model = null,
            SystemPrompt = SystemPrompt,
            Temperature = 0.2,
            MaxTokens = 1200
        };

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(90));

        using var client = new HttpClient();
        using var response = await client.PostAsJsonAsync(endpoint, payload, timeoutCts.Token);
        var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"LLM Gateway returned {(int)response.StatusCode}: {body}");
        }

        var result = JsonSerializer.Deserialize<LlmGatewayGenerateResponse>(body, JsonOptions);
        if (result is null || string.IsNullOrWhiteSpace(result.Content))
        {
            throw new InvalidOperationException("LLM Gateway returned empty response.");
        }

        return result;
    }

    private static string BuildPrompt(
        Doctor doctor,
        Patient? activePatient,
        IReadOnlyCollection<BotChatTurn> history,
        string userText)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Контекст врача:");
        sb.AppendLine($"- TelegramUserId: {doctor.TelegramUserId}");
        sb.AppendLine($"- Никнейм: {doctor.Registration?.Nickname ?? "не указан"}");
        sb.AppendLine($"- Специализация: {string.Join(", ", doctor.SpecializationTitles ?? new List<string>())}");
        sb.AppendLine();

        sb.AppendLine("Контекст пациента:");
        if (activePatient is null)
        {
            sb.AppendLine("- Активный пациент не выбран.");
        }
        else
        {
            sb.AppendLine($"- PatientId: {activePatient.Id}");
            sb.AppendLine($"- Никнейм: {activePatient.Nickname ?? "не указан"}");
            sb.AppendLine($"- Пол: {activePatient.Sex?.ToString() ?? "не указан"}");
            sb.AppendLine($"- Возраст: {(activePatient.AgeYears.HasValue ? activePatient.AgeYears.Value.ToString() : "не указан")}");
            sb.AppendLine($"- Аллергии: {activePatient.Allergies ?? "не указаны"}");
            sb.AppendLine($"- Хронические состояния: {activePatient.ChronicConditions ?? "не указаны"}");
            sb.AppendLine($"- Теги: {activePatient.Tags ?? "нет"}");
            sb.AppendLine($"- Заметки: {activePatient.Notes ?? "нет"}");
        }

        if (history.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("История диалога (последние ходы):");
            foreach (var turn in history)
            {
                sb.AppendLine($"Пользователь: {turn.UserText}");
                sb.AppendLine($"Ассистент: {turn.AssistantText}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Текущий вопрос пользователя:");
        sb.AppendLine(userText.Trim());

        return sb.ToString();
    }

    private static BotChatAnswerDto ToDto(BotChatTurn turn) =>
        new(
            turn.ConversationId,
            turn.RequestId,
            turn.AssistantText);

    private sealed record LlmGatewayGenerateRequest
    {
        public string Prompt { get; init; } = string.Empty;
        public string? Model { get; init; }
        public string? SystemPrompt { get; init; }
        public double? Temperature { get; init; }
        public int? MaxTokens { get; init; }
    }

    private sealed record LlmGatewayGenerateResponse(
        string Provider,
        string Model,
        string Content,
        string? FinishReason,
        int? PromptTokens,
        int? CompletionTokens,
        string? RequestId);
}
