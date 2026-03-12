namespace MedAssist.Domain.Entities;

public class BotConversationSummary
{
    public Guid ConversationId { get; init; }
    public BotConversation Conversation { get; set; } = null!;
    public Guid? PatientId { get; set; }
    public string? SpecialtyCode { get; set; }
    public string? SummaryText { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
