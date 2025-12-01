using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record AdminPatientDto(
    Guid Id,
    Guid DoctorId,
    string FullName,
    DateTime? BirthDate,
    PatientSex? Sex,
    string? Phone,
    string? Email,
    string? Allergies,
    string? ChronicConditions,
    string? Tags,
    PatientStatus Status,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid? LastDialogId,
    string? LastSummary,
    DateTimeOffset? LastInteractionAt);
