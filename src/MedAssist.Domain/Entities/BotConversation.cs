namespace MedAssist.Domain.Entities;

public class BotConversation
{
    public Guid Id { get; init; }
    public long TelegramUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public List<BotChatTurn> Turns { get; set; } = new();
}
