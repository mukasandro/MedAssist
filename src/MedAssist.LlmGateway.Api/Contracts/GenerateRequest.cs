using System.ComponentModel.DataAnnotations;

namespace MedAssist.LlmGateway.Api.Contracts;

public record GenerateRequest
{
    public string? Model { get; init; }

    [Range(0, 2)]
    public double? Temperature { get; init; }

    [Range(1, 8192)]
    public int? MaxTokens { get; init; }

    [Required]
    [MinLength(1)]
    public IReadOnlyList<ChatMessageRequest> Messages { get; init; } = Array.Empty<ChatMessageRequest>();
}
