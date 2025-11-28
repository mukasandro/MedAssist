using MedAssist.Application.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Swagger.Examples;

public class CreateMessageRequestExample : IExamplesProvider<CreateMessageRequest>
{
    public CreateMessageRequest GetExamples() =>
        new("doctor", "Расскажите, пожалуйста, о текущих жалобах.");
}
