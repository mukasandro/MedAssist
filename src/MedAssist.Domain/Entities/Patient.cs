using MedAssist.Domain.Enums;

namespace MedAssist.Domain.Entities;

public class Patient
{
    public Guid Id { get; init; }
    public Guid DoctorId { get; init; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public PatientSex? Sex { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? Tags { get; set; }
    public PatientStatus Status { get; set; } = PatientStatus.Active;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Guid? LastDialogId { get; set; }
    public string? LastSummary { get; set; }
    public DateTimeOffset? LastInteractionAt { get; set; }
}
