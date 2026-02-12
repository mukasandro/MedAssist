using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class SetActivePatientRequestValidator : AbstractValidator<SetActivePatientRequest>
{
    public SetActivePatientRequestValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
    }
}
