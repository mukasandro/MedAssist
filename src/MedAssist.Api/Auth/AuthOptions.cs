using System.ComponentModel.DataAnnotations;

namespace MedAssist.Api.Auth;

public class AuthOptions
{
    public const string SectionName = "Auth";

    public JwtOptions Jwt { get; set; } = new();

    public TelegramOptions Telegram { get; set; } = new();

    public ServiceOptions Service { get; set; } = new();
}

public class JwtOptions
{
    [Required]
    public string Issuer { get; set; } = "medassist-api";

    [Required]
    public string Audience { get; set; } = "medassist-clients";

    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = "dev-only-change-this-signing-key-32+";

    [Range(1, 1440)]
    public int AccessTokenMinutes { get; set; } = 60;
}

public class TelegramOptions
{
    public string BotToken { get; set; } = string.Empty;

    [Range(10, 3600)]
    public int InitDataMaxAgeSeconds { get; set; } = 3600;

    [Range(10, 3600)]
    public int ReplayProtectionTtlSeconds { get; set; } = 600;
}

public class ServiceOptions
{
    public string ClientId { get; set; } = "bot-service";

    public string ApiKey { get; set; } = string.Empty;

    public string? PreviousApiKey { get; set; }

    public string[] Scopes { get; set; } = ["bot"];
}
