using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IConsentService
{
    Task<IReadOnlyCollection<ConsentDto>> GetListAsync(CancellationToken cancellationToken);
    Task<ConsentDto> AddHumanInLoopAsync(CreateHumanInLoopConsentRequest request, CancellationToken cancellationToken);
}
