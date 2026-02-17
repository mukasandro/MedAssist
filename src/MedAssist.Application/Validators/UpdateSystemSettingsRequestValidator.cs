using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class UpdateSystemSettingsRequestValidator : AbstractValidator<UpdateSystemSettingsRequest>
{
    public UpdateSystemSettingsRequestValidator()
    {
        RuleFor(x => x.LlmGatewayUrl)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(BeAbsoluteHttpUrl)
            .WithMessage("llmGatewayUrl must be a valid absolute http/https URL.");
    }

    private static bool BeAbsoluteHttpUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsed))
        {
            return false;
        }

        return parsed.Scheme is "http" or "https";
    }
}
