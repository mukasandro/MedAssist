namespace MedAssist.LlmGateway.Api.Contracts;

public record ChatMessageRequest(
    string Role,
    string Content);
