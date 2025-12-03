using System.Collections.Generic;
using MedAssist.Domain.Enums;

namespace MedAssist.Domain.Entities;

public class Registration
{
    public RegistrationStatus Status { get; set; } = RegistrationStatus.NotStarted;
    public List<string> SpecializationCodes { get; set; } = new();
    public List<string> SpecializationTitles { get; set; } = new();
    public bool HumanInLoopConfirmed { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
}
