using MedAssist.Application.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Swagger.Examples;

public class CreatePatientRequestExample : IExamplesProvider<CreatePatientRequest>
{
    public CreatePatientRequest GetExamples() =>
        new(
            Sex: MedAssist.Domain.Enums.PatientSex.Male,
            AgeYears: 38,
            Nickname: "patient_038",
            Allergies: "Пенициллин",
            ChronicConditions: "Сахарный диабет 2 типа",
            Tags: "диабет,наблюдение",
            Notes: "Внутренняя пометка врача.",
            Status: MedAssist.Domain.Enums.PatientStatus.Active);
}
