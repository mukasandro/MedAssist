namespace MedAssist.Application.Requests;

public record CreateDialogRequest(Guid? PatientId, string? Topic);
