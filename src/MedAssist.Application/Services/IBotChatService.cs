using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IBotChatService
{
    Task<BotChatAnswerDto> AskAsync(AskBotQuestionRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BotConversationHistoryDto>> GetConversationsAsync(
        long? telegramUserId,
        int take,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BotChatTurnHistoryDto>> GetTurnsAsync(
        Guid conversationId,
        int take,
        CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BotChatTurnHistoryDto>> GetTurnsAsync(
        long telegramUserId,
        Guid conversationId,
        int take,
        CancellationToken cancellationToken);
}
