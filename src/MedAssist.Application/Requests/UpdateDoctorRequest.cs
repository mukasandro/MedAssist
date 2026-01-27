using System.Collections.Generic;

namespace MedAssist.Application.Requests;

public record UpdateDoctorRequest(
    IReadOnlyCollection<string>? SpecializationCodes,
    string? Nickname,
    bool Verified);
