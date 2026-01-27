using System.Collections.Generic;

namespace MedAssist.Application.Requests;

public record UpdateDoctorRequest(
    IReadOnlyCollection<string>? SpecializationCodes,
    long? TelegramUserId,
    string? Nickname,
    bool Verified);
