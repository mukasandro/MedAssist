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
[Route("v1/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [SwaggerOperation(Summary = "Список пациентов", Description = "Все пациенты врача по Telegram user id из заголовка.")]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<PatientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetList(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return BadRequest(new { error = "X-Telegram-User-Id header is required." });
        }

        var patients = await _patientService.GetListAsync(telegramUserId, cancellationToken);
        return Ok(patients);
    }

    [SwaggerOperation(Summary = "Пациент по id", Description = "Карточка пациента по Telegram user id из заголовка.")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        Guid id,
        CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return BadRequest(new { error = "X-Telegram-User-Id header is required." });
        }

        var patient = await _patientService.GetAsync(telegramUserId, id, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [SwaggerOperation(Summary = "Создать пациента", Description = "Добавить нового пациента.")]
    [HttpPost]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
    [SwaggerRequestExample(typeof(CreatePatientRequest), typeof(CreatePatientRequestExample))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        [FromBody] CreatePatientRequest request,
        CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return BadRequest(new { error = "X-Telegram-User-Id header is required." });
        }

        var patient = await _patientService.CreateAsync(telegramUserId, request, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(Get), new { id = patient.Id }, patient);
    }

    [SwaggerOperation(Summary = "Обновить пациента", Description = "Частичное обновление данных пациента.")]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerRequestExample(typeof(UpdatePatientRequest), typeof(UpdatePatientRequestExample))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        Guid id,
        [FromBody] UpdatePatientRequest request,
        CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return BadRequest(new { error = "X-Telegram-User-Id header is required." });
        }

        var patient = await _patientService.UpdateAsync(telegramUserId, id, request, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [SwaggerOperation(Summary = "Удалить пациента", Description = "Удаляет пациента врача.")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        Guid id,
        CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return BadRequest(new { error = "X-Telegram-User-Id header is required." });
        }

        await _patientService.DeleteAsync(telegramUserId, id, cancellationToken);
        return NoContent();
    }

    [Obsolete("Use PUT /v1/me/active-patient.")]
    [SwaggerOperation(
        Summary = "Установить активного пациента (устаревший)",
        Description = "Устаревший метод. Используйте PUT /v1/me/active-patient.")]
    [HttpPost("{id:guid}/setactive")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActive(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        Guid id,
        CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return BadRequest(new { error = "X-Telegram-User-Id header is required." });
        }

        var profile = await _patientService.SelectAsync(telegramUserId, id, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }
}
