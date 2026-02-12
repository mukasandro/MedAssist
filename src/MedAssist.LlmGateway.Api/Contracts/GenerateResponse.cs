namespace MedAssist.LlmGateway.Api.Contracts;

public record GenerateResponse(
    string Provider,
    string Model,
    string Content,
    string? FinishReason,
    int? PromptTokens,
    int? CompletionTokens,
    string? RequestId);
