namespace MedAssist.Application.DTOs;

public record StaticContentAdminDto(
    Guid Id,
    string Code,
    string? Title,
    string Value,
    DateTimeOffset UpdatedAt);
