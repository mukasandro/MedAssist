using System.Collections.Generic;

namespace MedAssist.Application.Requests;

public record CreateDoctorRequest(
    IReadOnlyCollection<string> SpecializationCodes,
    string? Nickname,
    bool Verified);
