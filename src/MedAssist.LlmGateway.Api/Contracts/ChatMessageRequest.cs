using System.ComponentModel.DataAnnotations;

namespace MedAssist.LlmGateway.Api.Contracts;

public record ChatMessageRequest
{
    [Required]
    [MinLength(1)]
    public string Role { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Content { get; init; } = string.Empty;
}
