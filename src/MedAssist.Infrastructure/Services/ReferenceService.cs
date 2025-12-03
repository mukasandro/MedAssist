using System.Collections.Immutable;
using MedAssist.Application.DTOs;
using MedAssist.Application.Services;

namespace MedAssist.Infrastructure.Services;

public class ReferenceService : IReferenceService
{
    private static ImmutableArray<SpecializationDto> _specializations = ImmutableArray.Create(
        new SpecializationDto("cardiology", "Cardiology"),
        new SpecializationDto("therapy", "Therapy / Internal medicine"),
        new SpecializationDto("neurology", "Neurology"),
        new SpecializationDto("pediatrics", "Pediatrics"),
        new SpecializationDto("dermatology", "Dermatology"),
        new SpecializationDto("psychiatry", "Psychiatry"));

    public Task<IReadOnlyCollection<SpecializationDto>> GetSpecializationsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyCollection<SpecializationDto>>(_specializations);
    }

    public Task<IReadOnlyCollection<SpecializationDto>> UpdateSpecializationsAsync(
        IReadOnlyCollection<SpecializationDto> specializations,
        CancellationToken cancellationToken)
    {
        _specializations = specializations is null
            ? ImmutableArray<SpecializationDto>.Empty
            : specializations.ToImmutableArray();

        return Task.FromResult<IReadOnlyCollection<SpecializationDto>>(_specializations);
    }
}
