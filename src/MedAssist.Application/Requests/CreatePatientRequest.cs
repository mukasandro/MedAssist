using MedAssist.Domain.Enums;

namespace MedAssist.Application.Requests;

public record CreatePatientRequest(
    PatientSex? Sex,
    int? AgeYears,
    string? Nickname,
    string? Allergies,
    string? ChronicConditions,
    string? Tags,
    string? Notes,
    PatientStatus? Status);
