using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record PatientDto(
    Guid Id,
    PatientSex? Sex,
    int? AgeYears,
    string? Nickname,
    string? Allergies,
    string? ChronicConditions,
    string? Tags,
    PatientStatus Status,
    string? Notes,
    DateTimeOffset? LastInteractionAt);
