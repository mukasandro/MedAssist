using System.Collections.Generic;

namespace MedAssist.Application.DTOs;

public record DoctorPublicDto(
    Guid Id,
    IReadOnlyCollection<string> SpecializationCodes,
    IReadOnlyCollection<string> SpecializationTitles,
    long? TelegramUserId,
    string? Nickname,
    bool Verified);
