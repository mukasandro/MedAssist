using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleForEach(x => x.SpecializationCodes!).MaximumLength(100).When(x => x.SpecializationCodes != null);
        RuleForEach(x => x.SpecializationTitles!).MaximumLength(256).When(x => x.SpecializationTitles != null);
        RuleFor(x => x)
            .Must(x => x.SpecializationCodes == null ||
                       (x.SpecializationTitles != null &&
                        x.SpecializationCodes.Count == x.SpecializationTitles.Count))
            .WithMessage("Количество кодов и названий специализаций должно совпадать.");
        RuleFor(x => x.Nickname).MaximumLength(64);
    }
}
