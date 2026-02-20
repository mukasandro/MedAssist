using MedAssist.Api.Contracts.Billing;
using MedAssist.Api.Extensions;
using MedAssist.Api.Swagger;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("bot")]
[Authorize(Policy = "MiniAppOrBot")]
[ApiController]
[Route("v1/me/billing")]
public class MeBillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public MeBillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpPost("topup")]
    [SwaggerOperation(
        Summary = "Пополнить свой баланс токенов",
        Description = "Пополняет токены текущего пользователя (telegramUserId берется из X-Telegram-User-Id или JWT claim).")]
    [ProducesResponseType(typeof(DoctorTokenBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TopUp(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        [FromBody] TopUpMyTokensRequest request,
        CancellationToken cancellationToken)
    {
        if (!this.TryResolveTelegramUserId(telegramUserId, out telegramUserId))
        {
            return BadRequest(new { error = "X-Telegram-User-Id header or JWT claim telegram_user_id is required." });
        }

        if (request.Tokens <= 0)
        {
            return BadRequest(new { error = "tokens must be greater than 0." });
        }

        var result = await _billingService.TopUpAsync(
            new TopUpDoctorTokensRequest(telegramUserId, request.Tokens),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("history")]
    [SwaggerOperation(
        Summary = "Моя история токенов",
        Description = "Возвращает историю списаний/пополнений только текущего пользователя.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BillingTokenLedgerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> History(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        if (!this.TryResolveTelegramUserId(telegramUserId, out telegramUserId))
        {
            return BadRequest(new { error = "X-Telegram-User-Id header or JWT claim telegram_user_id is required." });
        }

        var result = await _billingService.GetHistoryAsync(telegramUserId, take, cancellationToken);
        return Ok(result);
    }
}
