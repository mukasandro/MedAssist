using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MedAssist.LlmGateway.Api.Contracts;
using MedAssist.LlmGateway.Api.Options;
using Microsoft.Extensions.Options;

namespace MedAssist.LlmGateway.Api.Providers;

public abstract class OpenAiCompatibleChatProvider : ILLMProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<LlmGatewayOptions> _options;
    private readonly ILogger _logger;

    public abstract string Name { get; }

    protected OpenAiCompatibleChatProvider(
        HttpClient httpClient,
        IOptionsMonitor<LlmGatewayOptions> options,
        ILogger logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    protected abstract ProviderOptions GetProviderOptions(LlmGatewayOptions options);

    public async Task<GenerateResponse> GenerateAsync(ProviderGenerateRequest request, CancellationToken cancellationToken)
    {
        var gatewayOptions = _options.CurrentValue;
        var providerOptions = GetProviderOptions(gatewayOptions);

        if (string.IsNullOrWhiteSpace(providerOptions.ApiKey))
        {
            throw new InvalidOperationException($"{Name} api key is not configured.");
        }

        if (string.IsNullOrWhiteSpace(providerOptions.BaseUrl))
        {
            throw new InvalidOperationException($"{Name} base url is not configured.");
        }

        var messages = new List<object>();
        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new { role = "system", content = request.SystemPrompt });
        }

        messages.Add(new { role = "user", content = request.Prompt });

        var payload = new Dictionary<string, object?>
        {
            ["model"] = request.Model,
            ["messages"] = messages
        };

        if (request.Temperature.HasValue)
        {
            payload["temperature"] = request.Temperature.Value;
        }

        if (request.MaxTokens.HasValue)
        {
            payload["max_tokens"] = request.MaxTokens.Value;
        }

        var endpoint = providerOptions.BaseUrl.TrimEnd('/') + "/v1/chat/completions";
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", providerOptions.ApiKey);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(gatewayOptions.RequestTimeoutMs));

        using var response = await _httpClient.SendAsync(httpRequest, timeoutCts.Token);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("LLM provider {Provider} returned status {Status}: {Body}", Name, (int)response.StatusCode, body);
            throw new LlmProviderException($"{Name} request failed with status {(int)response.StatusCode}.", (int)response.StatusCode);
        }

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var requestId = root.TryGetProperty("id", out var idNode) ? idNode.GetString() : null;
        var model = root.TryGetProperty("model", out var modelNode)
            ? modelNode.GetString() ?? request.Model
            : request.Model;

        var choice = root.GetProperty("choices")[0];
        var finishReason = choice.TryGetProperty("finish_reason", out var finishNode)
            ? finishNode.GetString()
            : null;

        var messageNode = choice.GetProperty("message");
        var content = messageNode.TryGetProperty("content", out var contentNode)
            ? ExtractContent(contentNode)
            : string.Empty;

        int? promptTokens = null;
        int? completionTokens = null;
        if (root.TryGetProperty("usage", out var usageNode))
        {
            if (usageNode.TryGetProperty("prompt_tokens", out var promptNode) && promptNode.TryGetInt32(out var prompt))
            {
                promptTokens = prompt;
            }

            if (usageNode.TryGetProperty("completion_tokens", out var completionNode) && completionNode.TryGetInt32(out var completion))
            {
                completionTokens = completion;
            }
        }

        return new GenerateResponse(
            Name,
            model,
            content,
            finishReason,
            promptTokens,
            completionTokens,
            requestId);
    }

    private static string ExtractContent(JsonElement contentNode)
    {
        return contentNode.ValueKind switch
        {
            JsonValueKind.String => contentNode.GetString() ?? string.Empty,
            JsonValueKind.Array => string.Join(
                "",
                contentNode.EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.Object && item.TryGetProperty("text", out _))
                    .Select(item => item.GetProperty("text").GetString() ?? string.Empty)),
            _ => contentNode.ToString()
        };
    }
}
