using MedAssist.Application.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Swagger.Examples;

public class UpdateProfileRequestExample : IExamplesProvider<UpdateProfileRequest>
{
    public UpdateProfileRequest GetExamples() =>
        new(
            SpecializationCodes: new[] { "cardiology" },
            SpecializationTitles: new[] { "Кардиология" },
            Nickname: "Доктор_кардио",
            LastSelectedPatientId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
}
