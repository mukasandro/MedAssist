using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IPatientService
{
    Task<IReadOnlyCollection<PatientDto>> GetListAsync(long telegramUserId, CancellationToken cancellationToken);
    Task<PatientDto?> GetAsync(long telegramUserId, Guid id, CancellationToken cancellationToken);
    Task<PatientDto?> CreateAsync(long telegramUserId, CreatePatientRequest request, CancellationToken cancellationToken);
    Task<PatientDto?> UpdateAsync(long telegramUserId, Guid id, UpdatePatientRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(long telegramUserId, Guid id, CancellationToken cancellationToken);
    Task<ProfileDto?> SelectAsync(long telegramUserId, Guid id, CancellationToken cancellationToken);
}
