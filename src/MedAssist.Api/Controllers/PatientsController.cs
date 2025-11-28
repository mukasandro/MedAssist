using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Api.Swagger.Examples;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Controllers;

[ApiController]
[Route("v1/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [SwaggerOperation(Summary = "Список пациентов", Description = "Все пациенты врача с краткой сводкой последнего диалога.")]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(CancellationToken cancellationToken)
    {
        var patients = await _patientService.GetListAsync(cancellationToken);
        return Ok(patients);
    }

    [SwaggerOperation(Summary = "Пациент по id", Description = "Карточка пациента и последняя сводка диалога.")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetAsync(id, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [SwaggerOperation(Summary = "Создать пациента", Description = "Добавить нового пациента.")]
    [HttpPost]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
    [SwaggerRequestExample(typeof(CreatePatientRequest), typeof(CreatePatientRequestExample))]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var patient = await _patientService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = patient.Id }, patient);
    }

    [SwaggerOperation(Summary = "Удалить пациента", Description = "Удаляет пациента врача.")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _patientService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [SwaggerOperation(Summary = "Выбрать пациента", Description = "Отметить пациента как текущего в профиле.")]
    [HttpPost("{id:guid}/select")]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Select(Guid id, CancellationToken cancellationToken)
    {
        var profile = await _patientService.SelectAsync(id, cancellationToken);
        return Ok(profile);
    }
}
