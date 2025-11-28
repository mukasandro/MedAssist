using MedAssist.Application.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Swagger.Examples;

public class CreatePatientRequestExample : IExamplesProvider<CreatePatientRequest>
{
    public CreatePatientRequest GetExamples() =>
        new(
            FullName: "Иван Иванов",
            BirthDate: new DateTime(1985, 5, 12),
            Sex: MedAssist.Domain.Enums.PatientSex.Male,
            Phone: "+79990000000",
            Email: "ivan@example.com",
            Allergies: "Пенициллин",
            ChronicConditions: "Сахарный диабет 2 типа",
            Tags: "диабет,наблюдение",
            Notes: "Внутренняя пометка врача.",
            Status: MedAssist.Domain.Enums.PatientStatus.Active);
}
