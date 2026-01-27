using MedAssist.Application.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Swagger.Examples;

public class UpdateProfileRequestExample : IExamplesProvider<UpdateProfileRequest>
{
    public UpdateProfileRequest GetExamples() =>
        new(Nickname: "Доктор_кардио");
}
