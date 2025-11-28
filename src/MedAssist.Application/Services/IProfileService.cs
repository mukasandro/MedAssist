using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IProfileService
{
    Task<ProfileDto> GetAsync(CancellationToken cancellationToken);
    Task<ProfileDto> UpdateAsync(UpdateProfileRequest request, CancellationToken cancellationToken);
}
