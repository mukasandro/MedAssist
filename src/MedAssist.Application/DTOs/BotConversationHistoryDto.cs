namespace MedAssist.Application.DTOs;

public record BotConversationHistoryDto(
    Guid ConversationId,
    long TelegramUserId,
    int TurnsCount,
    string? LastUserText,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
