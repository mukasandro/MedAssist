using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class UpsertRegistrationRequestValidator : AbstractValidator<UpsertRegistrationRequest>
{
    public UpsertRegistrationRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.SpecializationCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SpecializationTitle).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Degrees).MaximumLength(128);
        RuleFor(x => x.ExperienceYears).GreaterThanOrEqualTo(0).When(x => x.ExperienceYears.HasValue);
        RuleFor(x => x.Languages).MaximumLength(128);
        RuleFor(x => x.Bio).MaximumLength(1024);
        RuleFor(x => x.FocusAreas).MaximumLength(512);
        RuleFor(x => x.Location).MaximumLength(128);
        RuleFor(x => x.ContactPolicy).MaximumLength(256);
        RuleFor(x => x.AvatarUrl).MaximumLength(512);
    }
}
