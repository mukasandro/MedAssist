using MedAssist.Api.Swagger;
using MedAssist.Application.DTOs;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("bot")]
[Authorize(Policy = "BotOnly")]
[ApiController]
[Route("v1/runtime/settings")]
public class RuntimeSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _systemSettingsService;

    public RuntimeSettingsController(ISystemSettingsService systemSettingsService)
    {
        _systemSettingsService = systemSettingsService;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Получить runtime-настройки",
        Description = "Read-only настройки для внутренних сервисов (например, URL LLM Gateway).")]
    [ProducesResponseType(typeof(SystemSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var settings = await _systemSettingsService.GetAsync(cancellationToken);
        return Ok(settings);
    }
}
