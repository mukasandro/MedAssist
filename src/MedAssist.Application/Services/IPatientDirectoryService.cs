using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IPatientDirectoryService
{
    Task<IReadOnlyCollection<PatientDirectoryDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<PatientDirectoryDto?> UpdateAsync(Guid id, UpdatePatientDirectoryRequest request, CancellationToken cancellationToken);
    Task<PatientDirectoryDto> CreateRandomAsync(CancellationToken cancellationToken);
}
