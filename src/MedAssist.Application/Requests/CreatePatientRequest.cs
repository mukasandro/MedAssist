using MedAssist.Domain.Enums;

namespace MedAssist.Application.Requests;

public record CreatePatientRequest(
    string FullName,
    DateTime? BirthDate,
    PatientSex? Sex,
    string? Phone,
    string? Email,
    string? Allergies,
    string? ChronicConditions,
    string? Tags,
    string? Notes,
    PatientStatus? Status);
