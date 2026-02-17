namespace MedAssist.Application.DTOs;

public record BotChatAnswerDto(
    Guid ConversationId,
    Guid RequestId,
    string Answer);
