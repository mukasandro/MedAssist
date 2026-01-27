using System.Collections.Generic;

namespace MedAssist.Application.DTOs;

public record DoctorPublicDto(
    Guid Id,
    IReadOnlyCollection<SpecializationDto> Specializations,
    long? TelegramUserId,
    string? Nickname,
    bool Verified);
