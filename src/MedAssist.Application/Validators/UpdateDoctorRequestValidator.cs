using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class UpdateDoctorRequestValidator : AbstractValidator<UpdateDoctorRequest>
{
    public UpdateDoctorRequestValidator()
    {
        RuleForEach(x => x.SpecializationCodes!).NotEmpty().MaximumLength(100).When(x => x.SpecializationCodes != null);
        RuleForEach(x => x.SpecializationTitles!).NotEmpty().MaximumLength(256).When(x => x.SpecializationTitles != null);
        RuleFor(x => x)
            .Must(x =>
                (x.SpecializationCodes is null && x.SpecializationTitles is null) ||
                (x.SpecializationCodes != null && x.SpecializationTitles != null &&
                 x.SpecializationCodes.Count == x.SpecializationTitles.Count))
            .WithMessage("Количество кодов и названий специализаций должно совпадать, если они переданы.");
        RuleFor(x => x.Nickname).MaximumLength(64);
    }
}
