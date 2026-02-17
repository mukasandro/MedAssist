namespace MedAssist.Application.DTOs;

public record DoctorTokenBalanceDto(
    Guid DoctorId,
    long TelegramUserId,
    int TokenBalance);
