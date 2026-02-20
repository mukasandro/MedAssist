using System.ComponentModel.DataAnnotations;

namespace MedAssist.Api.Contracts.BotChat;

public sealed record AskBotQuestionHttpRequest
{
    [Required]
    public string Text { get; init; } = string.Empty;

    public Guid? ConversationId { get; init; }

    public Guid RequestId { get; init; }
}
