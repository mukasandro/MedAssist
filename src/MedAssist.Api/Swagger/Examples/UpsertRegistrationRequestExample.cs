using MedAssist.Application.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Swagger.Examples;

public class UpsertRegistrationRequestExample : IExamplesProvider<UpsertRegistrationRequest>
{
    public UpsertRegistrationRequest GetExamples() =>
        new(
            SpecializationCodes: new[] { "cardiology" },
            TelegramUserId: 123456789,
            Nickname: "Доктор_кардио",
            Confirmed: true);
}
