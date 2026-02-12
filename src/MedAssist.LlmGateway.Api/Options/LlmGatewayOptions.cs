using System.ComponentModel.DataAnnotations;

namespace MedAssist.LlmGateway.Api.Options;

public class LlmGatewayOptions
{
    public const string SectionName = "LlmGateway";

    [Range(1000, 300000)]
    public int RequestTimeoutMs { get; set; } = 60000;

    public ProviderOptions DeepSeek { get; set; } = new();
}

public class ProviderOptions
{
    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    [Required]
    public string ApiKey { get; set; } = string.Empty;

    [Required]
    public string DefaultModel { get; set; } = string.Empty;
}
