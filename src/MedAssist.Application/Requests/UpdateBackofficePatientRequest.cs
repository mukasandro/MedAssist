using MedAssist.Domain.Enums;

namespace MedAssist.Application.Requests;

public record UpdatePatientAdminRequest(
    string FullName,
    DateTime? BirthDate,
    PatientSex? Sex,
    string? Phone,
    string? Email,
    string? Allergies,
    string? ChronicConditions,
    string? Tags,
    PatientStatus Status,
    string? Notes);
