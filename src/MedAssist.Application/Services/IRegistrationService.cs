using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IRegistrationService
{
    Task<RegistrationDto> UpsertAsync(UpsertRegistrationRequest request, CancellationToken cancellationToken);
    Task<RegistrationDto> GetAsync(CancellationToken cancellationToken);
}
