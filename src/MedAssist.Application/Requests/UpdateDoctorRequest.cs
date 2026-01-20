using System.Collections.Generic;

namespace MedAssist.Application.Requests;

public record UpdateDoctorRequest(
    IReadOnlyCollection<string>? SpecializationCodes,
    IReadOnlyCollection<string>? SpecializationTitles,
    string? Nickname,
    bool Verified);
