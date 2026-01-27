using MedAssist.Api.Swagger;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[SwaggerGroup("admin")]
[ApiController]
[Route("v1/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Список врачей", Description = "Все врачи с карточками.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<DoctorPublicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var doctors = await _doctorService.GetAllAsync(cancellationToken);
        return Ok(doctors);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Обновить данные врача", Description = "Редактирование карточки врача.")]
    [ProducesResponseType(typeof(DoctorPublicDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        var updated = await _doctorService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPut("{id:guid}/specialization")]
    [SwaggerOperation(Summary = "Обновить специализацию врача", Description = "Меняет специализацию по коду из справочника.")]
    [ProducesResponseType(typeof(DoctorPublicDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSpecialization(
        Guid id,
        [FromBody] UpdateSpecializationRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _doctorService.UpdateSpecializationAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Удалить врача", Description = "Удаляет врача и связанные данные.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _doctorService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("test")]
    [SwaggerOperation(Summary = "Создать тестового врача", Description = "Генерирует врача с рандомными данными (Bogus).")]
    [ProducesResponseType(typeof(DoctorPublicDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTest(CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.CreateRandomAsync(cancellationToken);
        return Ok(doctor);
    }
}
