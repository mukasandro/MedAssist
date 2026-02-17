namespace MedAssist.Application.Requests;

public record AskBotQuestionRequest(
    long TelegramUserId,
    string Text,
    Guid? ConversationId,
    Guid RequestId);
