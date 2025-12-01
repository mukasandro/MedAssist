using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IPatientAdminService
{
    Task<IReadOnlyCollection<AdminPatientDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<AdminPatientDto?> UpdateAsync(Guid id, UpdatePatientAdminRequest request, CancellationToken cancellationToken);
    Task<AdminPatientDto> CreateRandomAsync(CancellationToken cancellationToken);
}
