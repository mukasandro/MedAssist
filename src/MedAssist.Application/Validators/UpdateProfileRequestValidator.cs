using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName).MaximumLength(256);
        RuleForEach(x => x.SpecializationCodes!).MaximumLength(100).When(x => x.SpecializationCodes != null);
        RuleForEach(x => x.SpecializationTitles!).MaximumLength(256).When(x => x.SpecializationTitles != null);
        RuleFor(x => x)
            .Must(x => x.SpecializationCodes == null ||
                       (x.SpecializationTitles != null &&
                        x.SpecializationCodes.Count == x.SpecializationTitles.Count))
            .WithMessage("Количество кодов и названий специализаций должно совпадать.");
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
