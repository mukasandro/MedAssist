namespace MedAssist.Application.DTOs;

public record SystemSettingsDto(
    string? LlmGatewayUrl,
    int EnrichChatHistoryDepth,
    DateTimeOffset UpdatedAt);
