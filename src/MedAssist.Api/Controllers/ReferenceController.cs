using MedAssist.Api.Swagger;
using MedAssist.Application.DTOs;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("admin")]
[ApiController]
[Route("v1/reference")]
public class ReferenceController : ControllerBase
{
    private readonly IReferenceService _referenceService;

    public ReferenceController(IReferenceService referenceService)
    {
        _referenceService = referenceService;
    }

    [SwaggerOperation(Summary = "Справочник специализаций", Description = "Доступные специализации для регистрации.")]
    [SwaggerGroup("bot")]
    [HttpGet("specializations")]
    [ProducesResponseType(typeof(IReadOnlyCollection<SpecializationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSpecializations(CancellationToken cancellationToken)
    {
        var result = await _referenceService.GetSpecializationsAsync(cancellationToken);
        return Ok(result);
    }

    [SwaggerOperation(
        Summary = "Обновить справочник специализаций",
        Description = "Принимает тот же JSON, который возвращает GET /specializations, и заменяет справочник.")]
    [HttpPut("specializations")]
    [ProducesResponseType(typeof(IReadOnlyCollection<SpecializationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateSpecializations(
        [FromBody] IReadOnlyCollection<SpecializationDto> specializations,
        CancellationToken cancellationToken)
    {
        var result = await _referenceService.UpdateSpecializationsAsync(specializations, cancellationToken);
        return Ok(result);
    }
}
