namespace MedAssist.Application.Requests;

public record UpdateSystemSettingsRequest
{
    public string LlmGatewayUrl { get; init; } = string.Empty;
    public int EnrichChatHistoryDepth { get; init; } = 5;
}
