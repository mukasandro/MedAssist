using MedAssist.Application.DTOs;

namespace MedAssist.Application.Services;

public interface IReferenceService
{
    Task<IReadOnlyCollection<SpecializationDto>> GetSpecializationsAsync(CancellationToken cancellationToken);
}
