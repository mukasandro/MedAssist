using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Api.Swagger.Examples;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Controllers;

[ApiController]
[Route("v1/me")]
public class MeController : ControllerBase
{
    private readonly IProfileService _profileService;

    public MeController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [SwaggerOperation(Summary = "Профиль врача", Description = "Кто залогинен, как идёт регистрация, кто выбран из пациентов.")]
    [HttpGet]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var profile = await _profileService.GetAsync(cancellationToken);
        return Ok(profile);
    }

    [SwaggerOperation(Summary = "Обновить профиль", Description = "Изменить отображаемое имя и/или выбрать текущего пациента.")]
    [HttpPatch]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [SwaggerRequestExample(typeof(UpdateProfileRequest), typeof(UpdateProfileRequestExample))]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await _profileService.UpdateAsync(request, cancellationToken);
        return Ok(profile);
    }
}
