using System.Collections.Generic;
using Bogus;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

        patient.Sex = request.Sex;
        if (request.AgeYears.HasValue)
        {
            patient.AgeYears = request.AgeYears;
        }
        patient.Nickname = string.IsNullOrWhiteSpace(request.Nickname) ? null : request.Nickname.Trim();
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

        var patient = new Domain.Entities.Patient
        {
            Id = Guid.NewGuid(),
            DoctorId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Sex = faker.PickRandom<PatientSex>(new[] { PatientSex.Male, PatientSex.Female }),
            AgeYears = faker.Random.Int(18, 85),
            Nickname = $"patient_{faker.Random.Int(1000, 9999)}",
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

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (patient != null)
        {
            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync(cancellationToken);
        }
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
                SpecializationCodes = new List<string> { "therapy" },
                SpecializationTitles = new List<string> { "Терапия" },
                TelegramUserId = 1000000001,
                Registration = new Domain.Entities.Registration
                {
                    Status = Domain.Enums.RegistrationStatus.Completed,
                    SpecializationCodes = new List<string> { "therapy" },
                    SpecializationTitles = new List<string> { "Терапия" },
                    Confirmed = true,
                    StartedAt = DateTimeOffset.UtcNow
                }
            });
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private static PatientDirectoryDto ToDto(Domain.Entities.Patient patient) =>
        new(patient.Id, patient.DoctorId, patient.Sex, patient.AgeYears, patient.Nickname, patient.Allergies,
            patient.ChronicConditions, patient.Tags, patient.Status, patient.Notes, patient.CreatedAt, patient.UpdatedAt,
            patient.LastInteractionAt);
}
