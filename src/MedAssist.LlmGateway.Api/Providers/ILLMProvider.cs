using MedAssist.LlmGateway.Api.Contracts;

namespace MedAssist.LlmGateway.Api.Providers;

public interface ILLMProvider
{
    string Name { get; }

    Task<GenerateResponse> GenerateAsync(ProviderGenerateRequest request, CancellationToken cancellationToken);
}
