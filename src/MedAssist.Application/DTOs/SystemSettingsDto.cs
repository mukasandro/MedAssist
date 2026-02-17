namespace MedAssist.Application.DTOs;

public record SystemSettingsDto(
    string? LlmGatewayUrl,
    DateTimeOffset UpdatedAt);
