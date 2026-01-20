using System.Collections.Generic;

namespace MedAssist.Application.Requests;

public record UpdateProfileRequest(
    IReadOnlyCollection<string>? SpecializationCodes,
    IReadOnlyCollection<string>? SpecializationTitles,
    string? Nickname,
    Guid? LastSelectedPatientId);
