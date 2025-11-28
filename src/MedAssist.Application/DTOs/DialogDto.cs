using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record DialogDto(
    Guid Id,
    Guid? PatientId,
    string? Topic,
    DialogStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ClosedAt);
