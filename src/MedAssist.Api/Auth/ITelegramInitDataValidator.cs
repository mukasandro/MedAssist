namespace MedAssist.Api.Auth;

public interface ITelegramInitDataValidator
{
    bool TryValidate(string? initData, out long telegramUserId, out string error);
}
