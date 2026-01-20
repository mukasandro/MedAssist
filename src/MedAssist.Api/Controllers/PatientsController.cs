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
    public async Task<IActionResult> GetList(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        CancellationToken cancellationToken)
    {
        var patients = await _patientService.GetListAsync(telegramUserId, cancellationToken);
        return Ok(patients);
    }

    [SwaggerOperation(Summary = "Пациент по id", Description = "Карточка пациента по Telegram user id из заголовка.")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetAsync(telegramUserId, id, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [SwaggerOperation(Summary = "Создать пациента", Description = "Добавить нового пациента.")]
    [HttpPost]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
    [SwaggerRequestExample(typeof(CreatePatientRequest), typeof(CreatePatientRequestExample))]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        [FromBody] CreatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var patient = await _patientService.CreateAsync(telegramUserId, request, cancellationToken);
        if (patient is null)
        {
            return NotFound();
        }

        return CreatedAtAction(nameof(Get), new { id = patient.Id }, patient);
    }

    [SwaggerOperation(Summary = "Удалить пациента", Description = "Удаляет пациента врача.")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        Guid id,
        CancellationToken cancellationToken)
    {
        await _patientService.DeleteAsync(telegramUserId, id, cancellationToken);
        return NoContent();
    }

    [SwaggerOperation(Summary = "Установить активного пациента", Description = "Устанавливает активного пациента у доктора.")]
    [HttpPost("{id:guid}/setactive")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetActive(
        [FromHeader(Name = "X-Telegram-User-Id")] long telegramUserId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var profile = await _patientService.SelectAsync(telegramUserId, id, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }
}
