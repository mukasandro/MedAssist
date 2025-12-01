using MedAssist.Domain.Enums;

namespace MedAssist.Domain.Entities;

public class Doctor
{
    public Guid Id { get; init; }

    // Публичная карточка
    public string DisplayName { get; set; } = string.Empty;
    public string SpecializationCode { get; set; } = string.Empty;
    public string SpecializationTitle { get; set; } = string.Empty;
    public string TelegramUsername { get; set; } = string.Empty;
    public string? Degrees { get; set; }
    public int? ExperienceYears { get; set; }
    public string? Languages { get; set; }
    public string? Bio { get; set; }
    public string? FocusAreas { get; set; }
    public bool AcceptingNewPatients { get; set; } = true;
    public string? Location { get; set; }
    public string? ContactPolicy { get; set; }
    public string? AvatarUrl { get; set; }

    // Статусы и служебное
    public Registration Registration { get; set; } = new();
    public bool Verified { get; set; }
    public double? Rating { get; set; }
    public DateTimeOffset? RegisteredAt { get; set; }
    public DateTimeOffset? LastActiveAt { get; set; }

    // Внутренние поля
    public Guid? LastSelectedPatientId { get; set; }
    public string? Tags { get; set; }
    public List<Consent> Consents { get; set; } = new();

    public RegistrationStatus RegistrationStatus => Registration.Status;
}
