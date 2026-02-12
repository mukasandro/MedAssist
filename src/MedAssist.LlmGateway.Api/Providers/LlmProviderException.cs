namespace MedAssist.LlmGateway.Api.Providers;

public class LlmProviderException : Exception
{
    public int? StatusCode { get; }

    public LlmProviderException(string message, int? statusCode = null) : base(message)
    {
        StatusCode = statusCode;
    }
}
