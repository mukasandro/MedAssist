using MedAssist.Domain.Enums;

namespace MedAssist.Domain.Entities;

public class Registration
{
    public RegistrationStatus Status { get; set; } = RegistrationStatus.NotStarted;
    public string? Specialization { get; set; }
    public bool HumanInLoopConfirmed { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
}
