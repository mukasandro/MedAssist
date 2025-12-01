using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IDoctorAdminService
{
    Task<IReadOnlyCollection<DoctorPublicDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<DoctorPublicDto?> UpdateAsync(Guid id, UpdateDoctorRequest request, CancellationToken cancellationToken);
    Task<DoctorPublicDto> CreateRandomAsync(CancellationToken cancellationToken);
}
