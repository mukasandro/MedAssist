using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[ApiController]
[Route("v1/admin/patients")]
public class AdminPatientsController : ControllerBase
{
    private readonly IPatientAdminService _patientAdminService;

    public AdminPatientsController(IPatientAdminService patientAdminService)
    {
        _patientAdminService = patientAdminService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Список пациентов (админ)", Description = "Админ-доступ ко всем пациентам.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<AdminPatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var patients = await _patientAdminService.GetAllAsync(cancellationToken);
        return Ok(patients);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Обновить пациента (админ)", Description = "Редактирование пациента для админки.")]
    [ProducesResponseType(typeof(AdminPatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientAdminRequest request, CancellationToken cancellationToken)
    {
        var updated = await _patientAdminService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("test")]
    [SwaggerOperation(Summary = "Создать тестового пациента", Description = "Генерирует пациента с рандомными данными (Bogus).")]
    [ProducesResponseType(typeof(AdminPatientDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTest(CancellationToken cancellationToken)
    {
        var patient = await _patientAdminService.CreateRandomAsync(cancellationToken);
        return Ok(patient);
    }
}
