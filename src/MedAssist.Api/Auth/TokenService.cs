using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MedAssist.Api.Auth;

public sealed class TokenService : ITokenService
{
    private readonly IOptions<AuthOptions> _options;

    public TokenService(IOptions<AuthOptions> options)
    {
        _options = options;
    }

    public string CreateAccessToken(AuthSubject subject)
    {
        var settings = _options.Value.Jwt;
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject.TelegramUserId?.ToString() ?? subject.ClientId ?? "unknown"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(AuthClaimTypes.ActorType, subject.ActorType)
        };

        if (subject.TelegramUserId is not null)
        {
            claims.Add(new Claim(AuthClaimTypes.TelegramUserId, subject.TelegramUserId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(subject.ClientId))
        {
            claims.Add(new Claim(AuthClaimTypes.ClientId, subject.ClientId));
        }

        foreach (var scope in subject.Scopes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(AuthClaimTypes.Scope, scope));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int GetAccessTokenLifetimeSeconds()
    {
        return _options.Value.Jwt.AccessTokenMinutes * 60;
    }
}
