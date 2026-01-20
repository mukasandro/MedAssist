using MedAssist.Api.Swagger;
using MedAssist.Api.Swagger.Examples;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("bot")]
[ApiController]
[Route("v1/registration")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [SwaggerOperation(
        Summary = "Заполнить регистрацию врача",
        Description = "Никнейм, специализация и отметка подтверждения: если регистрации нет — создадим, если есть — дополним без потери уже введённых данных.")]
    [HttpPut]
    [ProducesResponseType(typeof(RegistrationDto), StatusCodes.Status200OK)]
    [SwaggerRequestExample(typeof(UpsertRegistrationRequest), typeof(UpsertRegistrationRequestExample))]
    public async Task<IActionResult> Upsert([FromBody] UpsertRegistrationRequest request, CancellationToken cancellationToken)
    {
        var result = await _registrationService.UpsertAsync(request, cancellationToken);
        return Ok(result);
    }

    [SwaggerOperation(Summary = "Отменить регистрацию", Description = "Сбрасывает регистрацию врача в состояние NotStarted.")]
    [HttpDelete]
    [ProducesResponseType(typeof(RegistrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unregister(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return BadRequest(new { error = "X-Telegram-User-Id header is required." });
        }

        var result = await _registrationService.UnregisterAsync(telegramUserId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
