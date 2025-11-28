namespace MedAssist.Application.Requests;

public record UpsertRegistrationRequest(
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
    bool HumanInLoopConfirmed);
