using MedAssist.LlmGateway.Api.Options;
using Microsoft.Extensions.Options;

namespace MedAssist.LlmGateway.Api.Providers;

public class DeepSeekProvider : OpenAiCompatibleChatProvider
{
    public const string ProviderName = "deepseek";

    public override string Name => ProviderName;

    public DeepSeekProvider(
        HttpClient httpClient,
        IOptionsMonitor<LlmGatewayOptions> options,
        ILogger<DeepSeekProvider> logger)
        : base(httpClient, options, logger)
    {
    }

    protected override ProviderOptions GetProviderOptions(LlmGatewayOptions options) => options.DeepSeek;
}
