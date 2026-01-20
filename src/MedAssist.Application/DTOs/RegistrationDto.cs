using System.Collections.Generic;
using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record RegistrationDto(
    RegistrationStatus Status,
    IReadOnlyCollection<string> SpecializationCodes,
    string? Nickname,
    bool Confirmed,
    DateTimeOffset? StartedAt,
    long? TelegramUserId);
