using System.Collections.Generic;
using MedAssist.Domain.Enums;

namespace MedAssist.Domain.Entities;

public class Doctor
{
    public Guid Id { get; init; }

    // Публичная карточка
    public List<string> SpecializationCodes { get; set; } = new();
    public List<string> SpecializationTitles { get; set; } = new();
    public long? TelegramUserId { get; set; }

    // Статусы и служебное
    public Registration Registration { get; set; } = new();
    public bool Verified { get; set; }
    public DateTimeOffset? RegisteredAt { get; set; }
    public DateTimeOffset? LastActiveAt { get; set; }

    // Внутренние поля
    public Guid? LastSelectedPatientId { get; set; }
    public string? Tags { get; set; }
    public List<Consent> Consents { get; set; } = new();
    public List<Patient> Patients { get; set; } = new();

    public RegistrationStatus RegistrationStatus => Registration.Status;
}
