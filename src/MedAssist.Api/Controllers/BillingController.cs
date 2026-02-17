using MedAssist.Api.Swagger;
using MedAssist.Api.Auth;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("admin")]
[SwaggerGroup("bot")]
[Authorize(Policy = "MiniAppOrBot")]
[ApiController]
[Route("v1/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpPost("topup")]
    [SwaggerOperation(
        Summary = "Пополнить токены врача",
        Description = "Увеличивает TokenBalance врача по Telegram user id.")]
    [ProducesResponseType(typeof(DoctorTokenBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TopUp([FromBody] TopUpDoctorTokensRequest request, CancellationToken cancellationToken)
    {
        var actorType = User.FindFirstValue(AuthClaimTypes.ActorType);
        if (string.Equals(actorType, AuthActorTypes.MiniApp, StringComparison.Ordinal))
        {
            var claimTelegramUserId = User.FindFirstValue(AuthClaimTypes.TelegramUserId);
            if (!long.TryParse(claimTelegramUserId, out var telegramUserIdFromToken) || telegramUserIdFromToken <= 0)
            {
                return Forbid();
            }

            if (request.TelegramUserId != telegramUserIdFromToken)
            {
                return Forbid();
            }
        }

        var result = await _billingService.TopUpAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("history")]
    [SwaggerOperation(
        Summary = "История токенов",
        Description = "Возвращает историю списаний/пополнений токенов.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BillingTokenLedgerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> History(
        [FromQuery] long? telegramUserId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var actorType = User.FindFirstValue(AuthClaimTypes.ActorType);
        if (string.Equals(actorType, AuthActorTypes.MiniApp, StringComparison.Ordinal))
        {
            var claimTelegramUserId = User.FindFirstValue(AuthClaimTypes.TelegramUserId);
            if (!long.TryParse(claimTelegramUserId, out var telegramUserIdFromToken) || telegramUserIdFromToken <= 0)
            {
                return Forbid();
            }

            if (telegramUserId.HasValue && telegramUserId.Value != telegramUserIdFromToken)
            {
                return Forbid();
            }

            telegramUserId = telegramUserIdFromToken;
        }

        var result = await _billingService.GetHistoryAsync(telegramUserId, take, cancellationToken);
        return Ok(result);
    }
}
