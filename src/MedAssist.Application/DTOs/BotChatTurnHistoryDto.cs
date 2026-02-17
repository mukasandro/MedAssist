namespace MedAssist.Application.DTOs;

public record BotChatTurnHistoryDto(
    Guid TurnId,
    Guid ConversationId,
    Guid RequestId,
    string UserText,
    string AssistantText,
    DateTimeOffset CreatedAt);
