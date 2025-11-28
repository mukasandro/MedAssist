using MedAssist.Domain.Enums;

namespace MedAssist.Domain.Entities;

public class Consent
{
    public Guid Id { get; init; }
    public Guid DoctorId { get; init; }
    public ConsentType Type { get; init; }
    public bool Accepted { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
