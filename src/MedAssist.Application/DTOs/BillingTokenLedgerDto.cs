namespace MedAssist.Application.DTOs;

public record BillingTokenLedgerDto(
    Guid Id,
    Guid DoctorId,
    long TelegramUserId,
    int Delta,
    int BalanceAfter,
    string Reason,
    Guid? ConversationId,
    Guid? ChatTurnId,
    Guid? RequestId,
    DateTimeOffset CreatedAt);
