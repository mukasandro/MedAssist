using MedAssist.Domain.Enums;

namespace MedAssist.Domain.Entities;

public class Patient
{
    public Guid Id { get; init; }
    public Guid DoctorId { get; init; }
    public Doctor Doctor { get; set; } = null!;
    public PatientSex? Sex { get; set; }
    public int? AgeYears { get; set; }
    public string? Nickname { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? Tags { get; set; }
    public PatientStatus Status { get; set; } = PatientStatus.Active;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastInteractionAt { get; set; }
}
