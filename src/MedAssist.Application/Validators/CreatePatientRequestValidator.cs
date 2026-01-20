using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class CreatePatientRequestValidator : AbstractValidator<CreatePatientRequest>
{
    public CreatePatientRequestValidator()
    {
        RuleFor(x => x.AgeYears).InclusiveBetween(0, 130).When(x => x.AgeYears.HasValue);
        RuleFor(x => x.Nickname).MaximumLength(64);
        RuleFor(x => x.Allergies).MaximumLength(512);
        RuleFor(x => x.ChronicConditions).MaximumLength(512);
        RuleFor(x => x.Tags).MaximumLength(256);
        RuleFor(x => x.Notes).MaximumLength(1024);
    }
}
