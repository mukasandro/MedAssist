using MedAssist.Application.DTOs;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

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
    [HttpGet("specializations")]
    [ProducesResponseType(typeof(IReadOnlyCollection<SpecializationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSpecializations(CancellationToken cancellationToken)
    {
        var result = await _referenceService.GetSpecializationsAsync(cancellationToken);
        return Ok(result);
    }
}
