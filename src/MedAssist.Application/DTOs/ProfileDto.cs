using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record ProfileDto(
    Guid DoctorId,
    string DisplayName,
    string SpecializationCode,
    string SpecializationTitle,
    string TelegramUsername,
    string? Degrees,
    int? ExperienceYears,
    string? Languages,
    string? Bio,
    string? FocusAreas,
    bool AcceptingNewPatients,
    string? Location,
    string? ContactPolicy,
    string? AvatarUrl,
    RegistrationStatus RegistrationStatus,
    Guid? LastSelectedPatientId,
    bool Verified,
    double? Rating);
