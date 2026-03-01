using System.Collections.Generic;

namespace MedAssist.Application.DTOs;

public record DoctorPublicDto(
    Guid Id,
    IReadOnlyCollection<SpecializationDto> Specializations,
    long? TelegramUserId,
    int TokenBalance,
    string? Nickname,
    Guid? LastSelectedPatientId,
    bool Verified);
