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

        if (request.Messages.Count == 0)
        {
            throw new ArgumentException("messages must contain at least one item.");
        }

        var normalizedMessages = new List<ChatMessageRequest>(request.Messages.Count);
        for (var i = 0; i < request.Messages.Count; i++)
        {
            var message = request.Messages[i];
            if (message is null)
            {
                throw new ArgumentException($"messages[{i}] is required.");
            }

            var role = message.Role?.Trim().ToLowerInvariant();
            if (role is not ("system" or "user" or "assistant"))
            {
                throw new ArgumentException($"messages[{i}].role must be one of: system, user, assistant.");
            }

            var content = message.Content?.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException($"messages[{i}].content must be provided.");
            }

            normalizedMessages.Add(new ChatMessageRequest
            {
                Role = role,
                Content = content
            });
        }

        var providerRequest = new ProviderGenerateRequest(
            model,
            normalizedMessages,
            request.Temperature,
            request.MaxTokens);

        return await _provider.GenerateAsync(providerRequest, cancellationToken);
    }
}
