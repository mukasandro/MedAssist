using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IPatientService
{
    Task<IReadOnlyCollection<PatientDto>> GetListAsync(CancellationToken cancellationToken);
    Task<PatientDto?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<PatientDto> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<ProfileDto> SelectAsync(Guid id, CancellationToken cancellationToken);
}
