using MedAssist.Domain.Enums;

namespace MedAssist.Application.DTOs;

public record ConsentDto(
    Guid Id,
    ConsentType Type,
    bool Accepted,
    DateTimeOffset CreatedAt);
