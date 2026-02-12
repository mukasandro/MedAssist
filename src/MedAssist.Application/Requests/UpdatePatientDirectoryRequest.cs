using MedAssist.Domain.Enums;

namespace MedAssist.Application.Requests;

public record UpdatePatientDirectoryRequest(
    Guid? DoctorId,
    PatientSex? Sex,
    int? AgeYears,
    string? Nickname,
    string? Allergies,
    string? ChronicConditions,
    string? Tags,
    PatientStatus Status,
    string? Notes);
