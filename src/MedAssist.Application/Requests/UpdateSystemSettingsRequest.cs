namespace MedAssist.Application.Requests;

public record UpdateSystemSettingsRequest
{
    public string LlmGatewayUrl { get; init; } = string.Empty;
    public string EnrichServiceUrl { get; init; } = "https://enrich.muk.i234.me";
    public int EnrichChatHistoryDepth { get; init; } = 5;
}
