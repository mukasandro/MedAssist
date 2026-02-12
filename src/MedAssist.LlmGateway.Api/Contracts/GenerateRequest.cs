using System.ComponentModel.DataAnnotations;

namespace MedAssist.LlmGateway.Api.Contracts;

public record GenerateRequest
{
    [Required]
    [MinLength(1)]
    public string Prompt { get; init; } = string.Empty;

    public string? Model { get; init; }

    public string? SystemPrompt { get; init; }

    [Range(0, 2)]
    public double? Temperature { get; init; }

    [Range(1, 8192)]
    public int? MaxTokens { get; init; }
}
