using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IProfileService
{
    Task<ProfileDto?> GetAsync(long telegramUserId, CancellationToken cancellationToken);
    Task<ProfileDto?> UpdateAsync(long telegramUserId, UpdateProfileRequest request, CancellationToken cancellationToken);
    Task<ProfileDto?> UpdateSpecializationAsync(long telegramUserId, UpdateSpecializationRequest request, CancellationToken cancellationToken);
    Task<bool> SetActivePatientAsync(long telegramUserId, Guid patientId, CancellationToken cancellationToken);
    Task<bool> ClearActivePatientAsync(long telegramUserId, CancellationToken cancellationToken);
}
