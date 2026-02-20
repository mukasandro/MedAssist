using System.ComponentModel.DataAnnotations;

namespace MedAssist.Api.Contracts.Billing;

public sealed record TopUpMyTokensRequest
{
    [Range(1, int.MaxValue)]
    public int Tokens { get; init; }
}
