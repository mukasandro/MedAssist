namespace MedAssist.Api.Contracts.Auth;

public sealed record IssueTokenResponse(
    string AccessToken,
    int ExpiresIn,
    string TokenType,
    string ActorType);
