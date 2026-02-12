using MedAssist.Application.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Swagger.Examples;

public class SetActivePatientRequestExample : IExamplesProvider<SetActivePatientRequest>
{
    public SetActivePatientRequest GetExamples() =>
        new(PatientId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
}
