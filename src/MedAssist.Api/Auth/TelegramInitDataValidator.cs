using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MedAssist.Api.Auth;

public sealed class TelegramInitDataValidator : ITelegramInitDataValidator
{
    private readonly IOptions<AuthOptions> _options;
    private readonly ILogger<TelegramInitDataValidator> _logger;

    public TelegramInitDataValidator(
        IOptions<AuthOptions> options,
        ILogger<TelegramInitDataValidator> logger)
    {
        _options = options;
        _logger = logger;
    }

    public bool TryValidate(string? initData, out long telegramUserId, out string error)
    {
        telegramUserId = 0;
        error = string.Empty;

        var settings = _options.Value.Telegram;
        if (string.IsNullOrWhiteSpace(settings.BotToken))
        {
            error = "Telegram bot token is not configured on server.";
            _logger.LogWarning("Telegram initData validation failed: bot token is not configured.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(initData))
        {
            error = "initData is required for type=telegram_init_data.";
            _logger.LogWarning("Telegram initData validation failed: initData is empty.");
            return false;
        }

        var rawQuery = initData.StartsWith('?') ? initData[1..] : initData;
        var parsed = QueryHelpers.ParseQuery(rawQuery);

        if (!parsed.TryGetValue("hash", out var hashValues))
        {
            error = "initData hash is missing.";
            _logger.LogWarning("Telegram initData validation failed: hash is missing.");
            return false;
        }

        var receivedHash = hashValues.ToString().Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(receivedHash))
        {
            error = "initData hash is empty.";
            _logger.LogWarning("Telegram initData validation failed: hash is empty.");
            return false;
        }

        if (!parsed.TryGetValue("auth_date", out var authDateValues)
            || !long.TryParse(authDateValues.ToString(), out var authDateUnix))
        {
            error = "initData auth_date is invalid.";
            _logger.LogWarning("Telegram initData validation failed: auth_date is invalid.");
            return false;
        }

        var authDate = DateTimeOffset.FromUnixTimeSeconds(authDateUnix);
        if (DateTimeOffset.UtcNow - authDate > TimeSpan.FromSeconds(settings.InitDataMaxAgeSeconds))
        {
            error = "initData has expired.";
            _logger.LogWarning(
                "Telegram initData validation failed: expired. authDate={AuthDateUtc}, now={NowUtc}, maxAgeSeconds={MaxAgeSeconds}",
                authDate,
                DateTimeOffset.UtcNow,
                settings.InitDataMaxAgeSeconds);
            return false;
        }

        var dataCheckString = string.Join(
            '\n',
            parsed
                .Where(static kv => kv.Key != "hash")
                .OrderBy(static kv => kv.Key, StringComparer.Ordinal)
                .Select(kv => $"{kv.Key}={kv.Value.ToString()}"));

        var calculatedHash = CalculateTelegramHash(settings.BotToken, dataCheckString);
        if (!FixedTimeEquals(receivedHash, calculatedHash))
        {
            error = "initData hash validation failed.";
            _logger.LogWarning(
                "Telegram initData validation failed: hash mismatch. receivedHash={ReceivedHash}, calculatedHash={CalculatedHash}, hasSignature={HasSignature}",
                receivedHash,
                calculatedHash,
                parsed.ContainsKey("signature"));
            return false;
        }

        if (!parsed.TryGetValue("user", out var userValues))
        {
            error = "initData user is missing.";
            _logger.LogWarning("Telegram initData validation failed: user is missing.");
            return false;
        }

        using var doc = JsonDocument.Parse(userValues.ToString());
        if (!doc.RootElement.TryGetProperty("id", out var idProp)
            || !idProp.TryGetInt64(out telegramUserId)
            || telegramUserId <= 0)
        {
            error = "initData user.id is invalid.";
            _logger.LogWarning("Telegram initData validation failed: user.id is invalid.");
            return false;
        }

        _logger.LogInformation("Telegram initData validation succeeded for telegramUserId={TelegramUserId}.", telegramUserId);

        return true;
    }

    private static string CalculateTelegramHash(string botToken, string dataCheckString)
    {
        byte[] secretKey;
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("WebAppData")))
        {
            secretKey = hmac.ComputeHash(Encoding.UTF8.GetBytes(botToken));
        }

        using var hashHmac = new HMACSHA256(secretKey);
        var hashBytes = hashHmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}
