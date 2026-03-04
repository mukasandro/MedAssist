namespace MedAssist.Application.DTOs;

public record SystemSettingsDto(
    string? LlmGatewayUrl,
    string EnrichServiceUrl,
    int EnrichChatHistoryDepth,
    DateTimeOffset UpdatedAt);
