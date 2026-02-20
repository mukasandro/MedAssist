using MedAssist.Api.Swagger;
using MedAssist.Api.Auth;
using MedAssist.Api.Extensions;
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
    public async Task<IActionResult> TopUp(
        [FromHeader(Name = "X-Telegram-User-Id")] long headerTelegramUserId,
        [FromBody] TopUpDoctorTokensRequest request,
        CancellationToken cancellationToken)
    {
        var actorType = User.FindFirstValue(AuthClaimTypes.ActorType);
        var hasResolvedTelegramUserId = this.TryResolveTelegramUserId(headerTelegramUserId, out var resolvedTelegramUserId);
        var targetTelegramUserId = request.TelegramUserId;

        if (string.Equals(actorType, AuthActorTypes.MiniApp, StringComparison.Ordinal))
        {
            if (!hasResolvedTelegramUserId)
            {
                return BadRequest(new { error = "X-Telegram-User-Id header or JWT claim telegram_user_id is required." });
            }

            if (request.TelegramUserId > 0 && request.TelegramUserId != resolvedTelegramUserId)
            {
                return Forbid();
            }

            targetTelegramUserId = resolvedTelegramUserId;
        }
        else if (targetTelegramUserId <= 0)
        {
            if (!hasResolvedTelegramUserId)
            {
                return BadRequest(new { error = "telegramUserId in body or X-Telegram-User-Id header/JWT claim is required." });
            }

            targetTelegramUserId = resolvedTelegramUserId;
        }

        var result = await _billingService.TopUpAsync(
            new TopUpDoctorTokensRequest(targetTelegramUserId, request.Tokens),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("history")]
    [SwaggerOperation(
        Summary = "История токенов",
        Description = "Возвращает историю списаний/пополнений токенов.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BillingTokenLedgerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> History(
        [FromHeader(Name = "X-Telegram-User-Id")] long headerTelegramUserId,
        [FromQuery] long? telegramUserId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var actorType = User.FindFirstValue(AuthClaimTypes.ActorType);
        var hasResolvedTelegramUserId = this.TryResolveTelegramUserId(headerTelegramUserId, out var resolvedTelegramUserId);

        if (string.Equals(actorType, AuthActorTypes.MiniApp, StringComparison.Ordinal))
        {
            if (!hasResolvedTelegramUserId)
            {
                return BadRequest(new { error = "X-Telegram-User-Id header or JWT claim telegram_user_id is required." });
            }

            if (telegramUserId.HasValue && telegramUserId.Value != resolvedTelegramUserId)
            {
                return Forbid();
            }

            telegramUserId = resolvedTelegramUserId;
        }
        else if (!telegramUserId.HasValue && hasResolvedTelegramUserId)
        {
            telegramUserId = resolvedTelegramUserId;
        }

        var result = await _billingService.GetHistoryAsync(telegramUserId, take, cancellationToken);
        return Ok(result);
    }
}
