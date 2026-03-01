using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class UpdateDoctorRequestValidator : AbstractValidator<UpdateDoctorRequest>
{
    public UpdateDoctorRequestValidator()
    {
        RuleFor(x => x.Nickname).MaximumLength(64);
        RuleFor(x => x.TelegramUserId)
            .GreaterThan(0)
            .When(x => x.TelegramUserId.HasValue);
        RuleForEach(x => x.SpecializationCodes!)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.SpecializationCodes != null);
        RuleFor(x => x.LastSelectedPatientId)
            .Must(value => string.IsNullOrWhiteSpace(value) || Guid.TryParse(value, out _))
            .WithMessage("lastSelectedPatientId must be a valid GUID.");
    }
}
