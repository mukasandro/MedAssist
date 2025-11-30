namespace MedAssist.Application.DTOs;

public record DoctorPublicDto(
    Guid Id,
    string DisplayName,
    string SpecializationCode,
    string SpecializationTitle,
    string? Degrees,
    int? ExperienceYears,
    string? Languages,
    string? Bio,
    string? FocusAreas,
    bool AcceptingNewPatients,
    string? Location,
    string? ContactPolicy,
    string? AvatarUrl,
    bool Verified,
    double? Rating);
