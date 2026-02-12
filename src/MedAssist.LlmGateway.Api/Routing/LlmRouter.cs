using MedAssist.LlmGateway.Api.Contracts;
using MedAssist.LlmGateway.Api.Options;
using MedAssist.LlmGateway.Api.Providers;
using Microsoft.Extensions.Options;

namespace MedAssist.LlmGateway.Api.Routing;

public class LlmRouter : ILLMRouter
{
    private readonly DeepSeekProvider _provider;
    private readonly IOptionsMonitor<LlmGatewayOptions> _options;

    public LlmRouter(DeepSeekProvider provider, IOptionsMonitor<LlmGatewayOptions> options)
    {
        _provider = provider;
        _options = options;
    }

    public async Task<GenerateResponse> GenerateAsync(GenerateRequest request, CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;
        var model = string.IsNullOrWhiteSpace(request.Model)
            ? options.DeepSeek.DefaultModel?.Trim() ?? string.Empty
            : request.Model.Trim();

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new InvalidOperationException("Default DeepSeek model is not configured.");
        }

        var providerRequest = new ProviderGenerateRequest(
            model,
            request.SystemPrompt,
            request.Prompt,
            request.Temperature,
            request.MaxTokens);

        return await _provider.GenerateAsync(providerRequest, cancellationToken);
    }
}
