using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class UpsertRegistrationRequestValidator : AbstractValidator<UpsertRegistrationRequest>
{
    public UpsertRegistrationRequestValidator()
    {
        RuleFor(x => x.SpecializationCodes).NotEmpty();
        RuleForEach(x => x.SpecializationCodes).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TelegramUserId).GreaterThan(0);
        RuleFor(x => x.Nickname).MaximumLength(64);
    }
}
