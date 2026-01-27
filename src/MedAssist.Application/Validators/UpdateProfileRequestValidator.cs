using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Nickname).MaximumLength(64);
    }
}
