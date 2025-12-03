using System.Collections.Generic;

namespace MedAssist.Application.Requests;

public record UpsertRegistrationRequest(
    string DisplayName,
    IReadOnlyCollection<string> SpecializationCodes,
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
    bool HumanInLoopConfirmed);
