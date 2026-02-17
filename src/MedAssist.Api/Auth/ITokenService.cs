namespace MedAssist.Api.Auth;

public interface ITokenService
{
    string CreateAccessToken(AuthSubject subject);

    int GetAccessTokenLifetimeSeconds();
}
