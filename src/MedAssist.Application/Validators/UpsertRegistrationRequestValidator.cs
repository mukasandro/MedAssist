using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class UpsertRegistrationRequestValidator : AbstractValidator<UpsertRegistrationRequest>
{
    public UpsertRegistrationRequestValidator()
    {
        RuleFor(x => x.TelegramUserId).GreaterThan(0);
        RuleFor(x => x.Nickname).MaximumLength(64);
        RuleForEach(x => x.SpecializationCodes!)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.SpecializationCodes != null);
    }
}
