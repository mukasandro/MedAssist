using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class CreatePatientRequestValidator : AbstractValidator<CreatePatientRequest>
{
    public CreatePatientRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(32);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Allergies).MaximumLength(512);
        RuleFor(x => x.ChronicConditions).MaximumLength(512);
        RuleFor(x => x.Tags).MaximumLength(256);
        RuleFor(x => x.Notes).MaximumLength(1024);
    }
}
