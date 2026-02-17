namespace MedAssist.Api.Auth;

public static class AuthGrantTypes
{
    public const string TelegramInitData = "telegram_init_data";
    public const string ApiKey = "api_key";
}

public static class AuthActorTypes
{
    public const string MiniApp = "miniapp";
    public const string BotService = "bot_service";
}

public static class AuthClaimTypes
{
    public const string ActorType = "actor_type";
    public const string TelegramUserId = "telegram_user_id";
    public const string Scope = "scope";
    public const string ClientId = "client_id";
}
