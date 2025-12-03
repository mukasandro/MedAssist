using System.Collections.Generic;

namespace MedAssist.Application.Requests;

public record UpdateProfileRequest(
    string? DisplayName,
    IReadOnlyCollection<string>? SpecializationCodes,
    IReadOnlyCollection<string>? SpecializationTitles,
    string? Degrees,
    int? ExperienceYears,
    string? Languages,
    string? Bio,
    string? FocusAreas,
    bool? AcceptingNewPatients,
    string? Location,
    string? ContactPolicy,
    string? AvatarUrl,
    Guid? LastSelectedPatientId);
