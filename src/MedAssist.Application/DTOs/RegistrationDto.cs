using System.Collections.Generic;
using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record RegistrationDto(
    RegistrationStatus Status,
    IReadOnlyCollection<string> SpecializationCodes,
    bool HumanInLoopConfirmed,
    DateTimeOffset? StartedAt,
    string TelegramUsername);
