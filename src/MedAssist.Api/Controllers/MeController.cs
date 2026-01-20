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
[Route("v1/me")]
public class MeController : ControllerBase
{
    private readonly IProfileService _profileService;

    public MeController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [SwaggerOperation(Summary = "Профиль врача", Description = "Кто залогинен и кто выбран из пациентов.")]
    [HttpGet]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return BadRequest(new { error = "X-Telegram-User-Id header is required." });
        }

        var profile = await _profileService.GetAsync(telegramUserId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [SwaggerOperation(Summary = "Обновить профиль", Description = "Изменить никнейм и/или выбрать текущего пациента.")]
    [HttpPatch]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [SwaggerRequestExample(typeof(UpdateProfileRequest), typeof(UpdateProfileRequestExample))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return BadRequest(new { error = "X-Telegram-User-Id header is required." });
        }

        var profile = await _profileService.UpdateAsync(telegramUserId, request, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [SwaggerOperation(Summary = "Сменить специализацию", Description = "Обновляет специализацию текущего врача по коду.")]
    [HttpPatch("specialization")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSpecialization(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        [FromBody] UpdateSpecializationRequest request,
        CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return BadRequest(new { error = "X-Telegram-User-Id header is required." });
        }

        try
        {
            var profile = await _profileService.UpdateSpecializationAsync(telegramUserId, request, cancellationToken);
            return profile is null ? NotFound() : Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
