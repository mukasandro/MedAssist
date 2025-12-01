using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[ApiController]
[Route("v1/patient-directory")]
public class PatientDirectoryController : ControllerBase
{
    private readonly IPatientDirectoryService _patientService;

    public PatientDirectoryController(IPatientDirectoryService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Список пациентов", Description = "Доступ ко всем пациентам.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PatientDirectoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var patients = await _patientService.GetAllAsync(cancellationToken);
        return Ok(patients);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Обновить пациента", Description = "Редактирование пациента.")]
    [ProducesResponseType(typeof(PatientDirectoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientDirectoryRequest request, CancellationToken cancellationToken)
    {
        var updated = await _patientService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("test")]
    [SwaggerOperation(Summary = "Создать тестового пациента", Description = "Генерирует пациента с рандомными данными (Bogus).")]
    [ProducesResponseType(typeof(PatientDirectoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTest(CancellationToken cancellationToken)
    {
        var patient = await _patientService.CreateRandomAsync(cancellationToken);
        return Ok(patient);
    }
}
