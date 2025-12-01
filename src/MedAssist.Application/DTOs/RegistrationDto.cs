using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record RegistrationDto(
    RegistrationStatus Status,
    string? Specialization,
    bool HumanInLoopConfirmed,
    DateTimeOffset? StartedAt,
    string TelegramUsername);
