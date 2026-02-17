using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MedAssist.Api.Contracts.Auth;

public sealed record IssueTokenRequest
{
    [Required]
    public string Type { get; init; } = string.Empty;

    public JsonElement Payload { get; init; }
}
