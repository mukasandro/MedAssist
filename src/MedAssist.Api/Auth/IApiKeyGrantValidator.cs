namespace MedAssist.Api.Auth;

public interface IApiKeyGrantValidator
{
    bool TryValidate(string? apiKey, out AuthSubject subject, out string error);
}
