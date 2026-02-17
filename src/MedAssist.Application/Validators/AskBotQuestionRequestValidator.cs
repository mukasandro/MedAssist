using FluentValidation;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Validators;

public class AskBotQuestionRequestValidator : AbstractValidator<AskBotQuestionRequest>
{
    public AskBotQuestionRequestValidator()
    {
        RuleFor(x => x.TelegramUserId)
            .GreaterThan(0);

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(8000);

        RuleFor(x => x.RequestId)
            .NotEqual(Guid.Empty);
    }
}
