namespace MedAssist.Domain.Entities;

public class BillingTokenLedger
{
    public Guid Id { get; init; }
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public long TelegramUserId { get; set; }
    public int Delta { get; set; }
    public int BalanceAfter { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? ConversationId { get; set; }
    public Guid? ChatTurnId { get; set; }
    public Guid? RequestId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
