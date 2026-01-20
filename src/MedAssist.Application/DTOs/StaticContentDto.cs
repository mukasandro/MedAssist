namespace MedAssist.Application.DTOs;

public record StaticContentDto(
    Guid Id,
    string Code,
    string? Title,
    string Value,
    DateTimeOffset UpdatedAt);
