using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class CreateDialogRequestValidator : AbstractValidator<CreateDialogRequest>
{
    public CreateDialogRequestValidator()
    {
        RuleFor(x => x.Topic).MaximumLength(256);
    }
}
