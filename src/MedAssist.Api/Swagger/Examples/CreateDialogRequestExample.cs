using MedAssist.Application.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Swagger.Examples;

public class CreateDialogRequestExample : IExamplesProvider<CreateDialogRequest>
{
    public CreateDialogRequest GetExamples() =>
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Сбор жалоб без привязки");
}
