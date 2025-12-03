using System.Collections.Generic;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Bogus;

namespace MedAssist.Infrastructure.Services;

public class PatientDirectoryService : IPatientDirectoryService
{
    private readonly MedAssistDbContext _db;

    public PatientDirectoryService(MedAssistDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<PatientDirectoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var patients = await _db.Patients.AsNoTracking().ToListAsync(cancellationToken);
        return patients.Select(ToDto).ToList();
    }

    public async Task<PatientDirectoryDto?> UpdateAsync(Guid id, UpdatePatientDirectoryRequest request, CancellationToken cancellationToken)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (patient == null) return null;

        patient.FullName = request.FullName;
        patient.BirthDate = request.BirthDate;
        patient.Sex = request.Sex;
        patient.Phone = request.Phone;
        patient.Email = request.Email;
        patient.Allergies = request.Allergies;
        patient.ChronicConditions = request.ChronicConditions;
        patient.Tags = request.Tags;
        patient.Status = request.Status;
        patient.Notes = request.Notes;
        patient.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(patient);
    }

    public async Task<PatientDirectoryDto> CreateRandomAsync(CancellationToken cancellationToken)
    {
        await EnsureDefaultDoctorAsync(cancellationToken);
        var faker = new Faker("ru");
        var birth = faker.Date.Past(50, DateTime.UtcNow.AddYears(-18));
        birth = DateTime.SpecifyKind(birth, DateTimeKind.Utc);

        var patient = new Domain.Entities.Patient
        {
            Id = Guid.NewGuid(),
            DoctorId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FullName = $"{faker.Name.LastName()} {faker.Name.FirstName()}",
            BirthDate = birth,
            Sex = faker.PickRandom<PatientSex>(new[] { PatientSex.Male, PatientSex.Female }),
            Phone = faker.Phone.PhoneNumber("+7 9## ### ## ##"),
            Email = faker.Internet.Email(),
            Allergies = faker.PickRandom(new[] { "Нет", "Пенициллин", "Пыльца", "Орехи" }),
            ChronicConditions = faker.PickRandom(new[] { "Гипертония", "Диабет 2 типа", "Нет данных" }),
            Tags = faker.PickRandom(new[] { "наблюдение", "плановый", "новый" }),
            Status = PatientStatus.Active,
            Notes = faker.Lorem.Sentence(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            LastInteractionAt = DateTimeOffset.UtcNow
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(patient);
    }

    private async Task EnsureDefaultDoctorAsync(CancellationToken cancellationToken)
    {
        var defId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var exists = await _db.Doctors.AnyAsync(d => d.Id == defId, cancellationToken);
        if (!exists)
        {
            _db.Doctors.Add(new Domain.Entities.Doctor
            {
                Id = defId,
                DisplayName = "Д-р Тестовый",
                SpecializationCodes = new List<string> { "therapy" },
                SpecializationTitles = new List<string> { "Терапия" },
                TelegramUsername = "test_doctor",
                AcceptingNewPatients = true,
                Languages = "ru",
                Registration = new Domain.Entities.Registration
                {
                    Status = Domain.Enums.RegistrationStatus.Completed,
                    SpecializationCodes = new List<string> { "therapy" },
                    SpecializationTitles = new List<string> { "Терапия" },
                    HumanInLoopConfirmed = true,
                    StartedAt = DateTimeOffset.UtcNow
                }
            });
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private static PatientDirectoryDto ToDto(Domain.Entities.Patient patient) =>
        new(patient.Id, patient.DoctorId, patient.FullName, patient.BirthDate, patient.Sex, patient.Phone, patient.Email,
            patient.Allergies, patient.ChronicConditions, patient.Tags, patient.Status, patient.Notes, patient.CreatedAt,
            patient.UpdatedAt, patient.LastDialogId, patient.LastSummary, patient.LastInteractionAt);
}
