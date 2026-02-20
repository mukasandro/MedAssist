using MedAssist.Api.Swagger;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("admin")]
[Authorize(Policy = "BotOnly")]
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
    public async Task<IActionResult> TopUp([FromBody] TopUpDoctorTokensRequest request, CancellationToken cancellationToken)
    {
        if (request.TelegramUserId <= 0)
        {
            return BadRequest(new { error = "telegramUserId must be greater than 0." });
        }

        if (request.Tokens <= 0)
        {
            return BadRequest(new { error = "tokens must be greater than 0." });
        }

        var result = await _billingService.TopUpAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("history")]
    [SwaggerOperation(
        Summary = "История токенов",
        Description = "Возвращает историю списаний/пополнений токенов. Можно фильтровать по Telegram user id.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<BillingTokenLedgerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> History(
        [FromQuery] long? telegramUserId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await _billingService.GetHistoryAsync(telegramUserId, take, cancellationToken);
        return Ok(result);
    }
}
