using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IDoctorService
{
    Task<IReadOnlyCollection<DoctorPublicDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<DoctorPublicDto?> UpdateAsync(Guid id, UpdateDoctorRequest request, CancellationToken cancellationToken);
    Task<DoctorPublicDto> CreateRandomAsync(CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
