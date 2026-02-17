namespace MedAssist.Application.Requests;

public record TopUpDoctorTokensRequest(
    long TelegramUserId,
    int Tokens);
