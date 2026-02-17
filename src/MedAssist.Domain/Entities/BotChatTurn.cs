namespace MedAssist.Domain.Entities;

public class BotChatTurn
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; set; }
    public BotConversation Conversation { get; set; } = null!;

    public Guid RequestId { get; set; }
    public string UserText { get; set; } = string.Empty;
    public string AssistantText { get; set; } = string.Empty;

    public string? Provider { get; set; }
    public string? Model { get; set; }
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public string? ProviderRequestId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
