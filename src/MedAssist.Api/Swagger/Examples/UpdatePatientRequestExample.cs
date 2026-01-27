using MedAssist.Application.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Swagger.Examples;

public class UpdatePatientRequestExample : IExamplesProvider<UpdatePatientRequest>
{
    public UpdatePatientRequest GetExamples() =>
        new(
            Sex: MedAssist.Domain.Enums.PatientSex.Female,
            AgeYears: 29,
            Nickname: "patient_029",
            Allergies: "Лактоза",
            ChronicConditions: "Гастрит",
            Tags: "контроль,питание",
            Notes: "Обновленные данные.",
            Status: MedAssist.Domain.Enums.PatientStatus.Active);
}
