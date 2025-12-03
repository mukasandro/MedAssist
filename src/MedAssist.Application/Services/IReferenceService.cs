using MedAssist.Application.DTOs;

namespace MedAssist.Application.Services;

public interface IReferenceService
{
    Task<IReadOnlyCollection<SpecializationDto>> GetSpecializationsAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SpecializationDto>> UpdateSpecializationsAsync(
        IReadOnlyCollection<SpecializationDto> specializations,
        CancellationToken cancellationToken);
}
