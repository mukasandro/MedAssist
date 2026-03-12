namespace MedAssist.Application.DTOs;

public record BotConversationHistoryDto(
    Guid ConversationId,
    long TelegramUserId,
    int TurnsCount,
    string? LastUserText,
    Guid? SummaryPatientId,
    string? SummarySpecialtyCode,
    string? SummaryText,
    DateTimeOffset? SummaryUpdatedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
