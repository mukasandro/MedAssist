namespace MedAssist.Application.Requests;

public record SetActivePatientRequest(
    Guid PatientId);
