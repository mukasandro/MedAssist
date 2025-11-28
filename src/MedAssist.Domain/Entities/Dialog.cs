using MedAssist.Domain.Enums;

namespace MedAssist.Domain.Entities;

public class Dialog
{
    public Guid Id { get; init; }
    public Guid DoctorId { get; init; }
    public Guid? PatientId { get; init; }
    public string? Topic { get; set; }
    public DialogStatus Status { get; set; } = DialogStatus.Open;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ClosedAt { get; set; }
}
