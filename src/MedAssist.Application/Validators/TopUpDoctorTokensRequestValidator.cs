using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class TopUpDoctorTokensRequestValidator : AbstractValidator<TopUpDoctorTokensRequest>
{
    public TopUpDoctorTokensRequestValidator()
    {
        RuleFor(x => x.TelegramUserId)
            .GreaterThan(0);

        RuleFor(x => x.Tokens)
            .GreaterThan(0)
            .LessThanOrEqualTo(1_000_000);
    }
}
