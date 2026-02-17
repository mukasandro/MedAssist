using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface ISystemSettingsService
{
    Task<SystemSettingsDto> GetAsync(CancellationToken cancellationToken);
    Task<SystemSettingsDto> UpdateAsync(UpdateSystemSettingsRequest request, CancellationToken cancellationToken);
    Task<string?> GetLlmGatewayUrlAsync(CancellationToken cancellationToken);
}
