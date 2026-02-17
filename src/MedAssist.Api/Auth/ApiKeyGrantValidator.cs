using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace MedAssist.Api.Auth;

public sealed class ApiKeyGrantValidator : IApiKeyGrantValidator
{
    private readonly IOptions<AuthOptions> _options;

    public ApiKeyGrantValidator(IOptions<AuthOptions> options)
    {
        _options = options;
    }

    public bool TryValidate(string? apiKey, out AuthSubject subject, out string error)
    {
        subject = default!;
        error = string.Empty;

        var inputKey = apiKey?.Trim();
        if (string.IsNullOrWhiteSpace(inputKey))
        {
            error = "apiKey is required for type=api_key.";
            return false;
        }

        var settings = _options.Value.Service;
        var configuredKey = settings.ApiKey.Trim();
        var previousKey = settings.PreviousApiKey?.Trim();

        if (string.IsNullOrWhiteSpace(configuredKey) && string.IsNullOrWhiteSpace(previousKey))
        {
            error = "Service API key is not configured on server.";
            return false;
        }

        var matched = FixedTimeEquals(inputKey, configuredKey)
                      || (!string.IsNullOrWhiteSpace(previousKey) && FixedTimeEquals(inputKey, previousKey));

        if (!matched)
        {
            error = "Invalid apiKey.";
            return false;
        }

        subject = new AuthSubject(
            AuthActorTypes.BotService,
            TelegramUserId: null,
            ClientId: settings.ClientId,
            Scopes: settings.Scopes.Where(static s => !string.IsNullOrWhiteSpace(s)).ToArray());

        return true;
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
