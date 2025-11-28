using MedAssist.Application.DTOs;
using MedAssist.Application.Services;

namespace MedAssist.Infrastructure.Services;

public class ReferenceService : IReferenceService
{
    private static readonly IReadOnlyCollection<SpecializationDto> Specializations =
        new List<SpecializationDto>
        {
            new("cardiology", "Cardiology"),
            new("therapy", "Therapy / Internal medicine"),
            new("neurology", "Neurology"),
            new("pediatrics", "Pediatrics"),
            new("dermatology", "Dermatology"),
            new("psychiatry", "Psychiatry")
        }.AsReadOnly();

    public Task<IReadOnlyCollection<SpecializationDto>> GetSpecializationsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Specializations);
    }
}
