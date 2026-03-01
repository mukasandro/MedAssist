using MedAssist.LlmGateway.Api.Contracts;

namespace MedAssist.LlmGateway.Api.Providers;

public record ProviderGenerateRequest(
    string Model,
    IReadOnlyList<ChatMessageRequest> Messages,
    double? Temperature,
    int? MaxTokens);
