namespace MedAssist.Api.Auth;

public sealed record AuthSubject(
    string ActorType,
    long? TelegramUserId,
    string? ClientId,
    IReadOnlyCollection<string> Scopes);
