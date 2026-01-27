using System.Collections.Generic;
using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record RegistrationDto(
    RegistrationStatus Status,
    IReadOnlyCollection<SpecializationDto> Specializations,
    string? Nickname,
    DateTimeOffset? StartedAt,
    long? TelegramUserId);
