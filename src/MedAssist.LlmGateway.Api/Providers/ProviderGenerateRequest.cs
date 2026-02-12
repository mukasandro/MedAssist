namespace MedAssist.LlmGateway.Api.Providers;

public record ProviderGenerateRequest(
    string Model,
    string? SystemPrompt,
    string Prompt,
    double? Temperature,
    int? MaxTokens);
