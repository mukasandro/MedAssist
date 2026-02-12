using MedAssist.LlmGateway.Api.Contracts;

namespace MedAssist.LlmGateway.Api.Routing;

public interface ILLMRouter
{
    Task<GenerateResponse> GenerateAsync(GenerateRequest request, CancellationToken cancellationToken);
}
