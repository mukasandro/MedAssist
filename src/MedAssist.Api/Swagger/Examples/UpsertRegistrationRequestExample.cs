using MedAssist.Application.Requests;
using Swashbuckle.AspNetCore.Filters;

namespace MedAssist.Api.Swagger.Examples;

public class UpsertRegistrationRequestExample : IExamplesProvider<UpsertRegistrationRequest>
{
    public UpsertRegistrationRequest GetExamples() =>
        new(
            DisplayName: "Д-р Иван Петров",
            SpecializationCodes: new[] { "cardiology" },
            TelegramUsername: "dr_ivan_petrov",
            Degrees: "к.м.н.",
            ExperienceYears: 12,
            Languages: "ru,en",
            Bio: "Кардиолог профилактического профиля.",
            FocusAreas: "профилактика,гипертензия",
            AcceptingNewPatients: true,
            Location: "Москва (MSK)",
            ContactPolicy: "Отвечаю в рабочие часы 10-18 MSK",
            AvatarUrl: "https://example.com/avatar.jpg",
            HumanInLoopConfirmed: true);
}
