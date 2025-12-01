using MedAssist.Application.DTOs;
using MedAssist.Application.Services;
using MedAssist.Application.Requests;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MedAssist.Api.Controllers;

[ApiController]
[Route("v1/admin/doctors")]
public class AdminDoctorsController : ControllerBase
{
    private readonly IDoctorAdminService _doctorAdminService;

    public AdminDoctorsController(IDoctorAdminService doctorAdminService)
    {
        _doctorAdminService = doctorAdminService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Список врачей (админ)", Description = "Для администраторов: все врачи с карточками.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<DoctorPublicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var doctors = await _doctorAdminService.GetAllAsync(cancellationToken);
        return Ok(doctors);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Обновить данные врача", Description = "Админ-редактирование карточки врача.")]
    [ProducesResponseType(typeof(DoctorPublicDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        var updated = await _doctorAdminService.UpdateAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("test")]
    [SwaggerOperation(Summary = "Создать тестового врача", Description = "Генерирует врача с рандомными данными (Bogus).")]
    [ProducesResponseType(typeof(DoctorPublicDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTest(CancellationToken cancellationToken)
    {
        var doctor = await _doctorAdminService.CreateRandomAsync(cancellationToken);
        return Ok(doctor);
    }
}
