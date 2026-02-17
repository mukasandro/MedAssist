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
[Route("v1/settings")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _systemSettingsService;

    public SystemSettingsController(ISystemSettingsService systemSettingsService)
    {
        _systemSettingsService = systemSettingsService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Получить системные настройки", Description = "Возвращает единый объект настроек приложения.")]
    [ProducesResponseType(typeof(SystemSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var settings = await _systemSettingsService.GetAsync(cancellationToken);
        return Ok(settings);
    }

    [HttpPut]
    [SwaggerOperation(Summary = "Обновить системные настройки", Description = "Обновляет единый объект настроек приложения.")]
    [ProducesResponseType(typeof(SystemSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] UpdateSystemSettingsRequest request, CancellationToken cancellationToken)
    {
        var settings = await _systemSettingsService.UpdateAsync(request, cancellationToken);
        return Ok(settings);
    }
}
