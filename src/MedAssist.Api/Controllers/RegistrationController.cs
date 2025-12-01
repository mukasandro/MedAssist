using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Api.Swagger.Examples;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Controllers;

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
        Description = "Имя, специализация и параметры карточки врача плюс отметка human-in-loop: если регистрации нет — создадим, если есть — дополним без потери уже введённых данных.")]
    [HttpPost]
    [ProducesResponseType(typeof(RegistrationDto), StatusCodes.Status200OK)]
    [SwaggerRequestExample(typeof(UpsertRegistrationRequest), typeof(UpsertRegistrationRequestExample))]
    public async Task<IActionResult> Upsert([FromBody] UpsertRegistrationRequest request, CancellationToken cancellationToken)
    {
        var result = await _registrationService.UpsertAsync(request, cancellationToken);
        return Ok(result);
    }

    [SwaggerOperation(Summary = "Статус регистрации", Description = "Возвращает текущий статус и заполненные поля.")]
    [HttpGet]
    [ProducesResponseType(typeof(RegistrationDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _registrationService.GetAsync(cancellationToken);
        return Ok(result);
    }

    [SwaggerOperation(Summary = "Статус регистрации по Telegram username", Description = "Поиск регистрации врача по его Telegram username.")]
    [HttpGet("{username}")]
    [ProducesResponseType(typeof(RegistrationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByTelegramUsername(string username, CancellationToken cancellationToken)
    {
        var result = await _registrationService.GetByTelegramUsernameAsync(username, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
