using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class CreateStaticContentRequestValidator : AbstractValidator<CreateStaticContentRequest>
{
    public CreateStaticContentRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Title).MaximumLength(128);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(4000);
    }
}
