using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record PatientDirectoryDto(
    Guid Id,
    Guid DoctorId,
    PatientSex? Sex,
    int? AgeYears,
    string? Nickname,
    string? Allergies,
    string? ChronicConditions,
    string? Tags,
    PatientStatus Status,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastInteractionAt);
