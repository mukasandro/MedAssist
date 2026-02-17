using MedAssist.Api.Auth;
using MedAssist.Api.Contracts.Auth;
using MedAssist.Api.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("bot")]
[ApiController]
[Route("v1/auth")]
public class AuthController : ControllerBase
{
    private readonly ITelegramInitDataValidator _telegramValidator;
    private readonly IApiKeyGrantValidator _apiKeyValidator;
    private readonly ITokenService _tokenService;

    public AuthController(
        ITelegramInitDataValidator telegramValidator,
        IApiKeyGrantValidator apiKeyValidator,
        ITokenService tokenService)
    {
        _telegramValidator = telegramValidator;
        _apiKeyValidator = apiKeyValidator;
        _tokenService = tokenService;
    }

    [HttpPost("token")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Получить JWT",
        Description = "Единый вход для Mini App и сервисного бота. Для api_key ключ передается только через Authorization: ApiKey ...")]
    [ProducesResponseType(typeof(IssueTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult IssueToken([FromBody] IssueTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Type))
        {
            return BadRequest(new { error = "type is required." });
        }

        var grantType = request.Type.Trim().ToLowerInvariant();
        AuthSubject subject;

        switch (grantType)
        {
            case AuthGrantTypes.TelegramInitData:
                if (!TryReadTelegramPayload(request.Payload, out var initData, out var payloadError))
                {
                    return BadRequest(new { error = payloadError });
                }

                if (!_telegramValidator.TryValidate(initData, out var telegramUserId, out var telegramError))
                {
                    return Unauthorized(new { error = telegramError });
                }

                subject = new AuthSubject(
                    AuthActorTypes.MiniApp,
                    TelegramUserId: telegramUserId,
                    ClientId: null,
                    Scopes: ["bot"]);
                break;

            case AuthGrantTypes.ApiKey:
                if (!TryValidateApiKeyPayload(request.Payload, out payloadError))
                {
                    return BadRequest(new { error = payloadError });
                }

                if (!TryReadApiKeyFromAuthorization(Request.Headers.Authorization.ToString(), out var apiKey))
                {
                    return Unauthorized(new { error = "Authorization header with ApiKey scheme is required." });
                }

                if (!_apiKeyValidator.TryValidate(apiKey, out subject!, out var apiKeyError))
                {
                    return Unauthorized(new { error = apiKeyError });
                }

                break;

            default:
                return BadRequest(new { error = "Unsupported type. Use telegram_init_data or api_key." });
        }

        var accessToken = _tokenService.CreateAccessToken(subject);
        var response = new IssueTokenResponse(
            AccessToken: accessToken,
            ExpiresIn: _tokenService.GetAccessTokenLifetimeSeconds(),
            TokenType: "Bearer",
            ActorType: subject.ActorType);

        return Ok(response);
    }

    private static bool TryReadTelegramPayload(JsonElement payload, out string initData, out string error)
    {
        initData = string.Empty;
        error = string.Empty;

        if (payload.ValueKind != JsonValueKind.Object)
        {
            error = "payload must be an object.";
            return false;
        }

        foreach (var property in payload.EnumerateObject())
        {
            if (!property.NameEquals("initData"))
            {
                error = "payload contains unsupported fields for telegram_init_data.";
                return false;
            }
        }

        if (!payload.TryGetProperty("initData", out var initDataElement) || initDataElement.ValueKind != JsonValueKind.String)
        {
            error = "payload.initData is required for telegram_init_data.";
            return false;
        }

        initData = initDataElement.GetString()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(initData))
        {
            error = "payload.initData is required for telegram_init_data.";
            return false;
        }

        return true;
    }

    private static bool TryValidateApiKeyPayload(JsonElement payload, out string error)
    {
        error = string.Empty;

        if (payload.ValueKind != JsonValueKind.Object)
        {
            error = "payload must be an object.";
            return false;
        }

        if (payload.EnumerateObject().Any())
        {
            error = "payload must be an empty object for api_key.";
            return false;
        }

        return true;
    }

    private static bool TryReadApiKeyFromAuthorization(string authorizationHeader, out string apiKey)
    {
        apiKey = string.Empty;
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return false;
        }

        const string prefix = "ApiKey ";
        if (!authorizationHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        apiKey = authorizationHeader[prefix.Length..].Trim();
        return !string.IsNullOrWhiteSpace(apiKey);
    }
}
