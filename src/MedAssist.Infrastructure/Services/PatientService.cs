using System.Collections.Generic;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedAssist.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly MedAssistDbContext _db;

    public PatientService(MedAssistDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<PatientDto>> GetListAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        var doctor = await GetDoctorAsync(telegramUserId, cancellationToken);
        if (doctor is null)
        {
            return Array.Empty<PatientDto>();
        }

        var patients = await _db.Patients
            .Where(p => p.DoctorId == doctor.Id)
            .OrderByDescending(p => p.LastInteractionAt ?? p.CreatedAt)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return patients.Select(ToDto).ToList().AsReadOnly();
    }

    public async Task<PatientDto?> GetAsync(long telegramUserId, Guid id, CancellationToken cancellationToken)
    {
        var doctor = await GetDoctorAsync(telegramUserId, cancellationToken);
        if (doctor is null)
        {
            return null;
        }

        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctor.Id, cancellationToken);
        return patient is null ? null : ToDto(patient);
    }

    public async Task<PatientDto?> CreateAsync(
        long telegramUserId,
        CreatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var doctor = await GetDoctorAsync(telegramUserId, cancellationToken);
        if (doctor is null)
        {
            return null;
        }

        var patient = new Domain.Entities.Patient
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            Sex = request.Sex,
            AgeYears = request.AgeYears,
            Nickname = string.IsNullOrWhiteSpace(request.Nickname) ? null : request.Nickname.Trim(),
            Allergies = request.Allergies,
            ChronicConditions = request.ChronicConditions,
            Tags = request.Tags,
            Status = request.Status ?? PatientStatus.Active,
            Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            LastInteractionAt = DateTimeOffset.UtcNow
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(patient);
    }

    public async Task<PatientDto?> UpdateAsync(
        long telegramUserId,
        Guid id,
        UpdatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var doctor = await GetDoctorAsync(telegramUserId, cancellationToken);
        if (doctor is null)
        {
            return null;
        }

        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctor.Id, cancellationToken);
        if (patient is null)
        {
            return null;
        }

        if (request.Sex.HasValue)
        {
            patient.Sex = request.Sex;
        }

        if (request.AgeYears.HasValue)
        {
            patient.AgeYears = request.AgeYears;
        }

        if (request.Nickname is not null)
        {
            patient.Nickname = string.IsNullOrWhiteSpace(request.Nickname) ? null : request.Nickname.Trim();
        }

        if (request.Allergies is not null)
        {
            patient.Allergies = request.Allergies;
        }

        if (request.ChronicConditions is not null)
        {
            patient.ChronicConditions = request.ChronicConditions;
        }

        if (request.Tags is not null)
        {
            patient.Tags = request.Tags;
        }

        if (request.Notes is not null)
        {
            patient.Notes = request.Notes;
        }

        if (request.Status.HasValue)
        {
            patient.Status = request.Status.Value;
        }

        patient.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(patient);
    }

    public async Task DeleteAsync(long telegramUserId, Guid id, CancellationToken cancellationToken)
    {
        var doctor = await GetDoctorAsync(telegramUserId, cancellationToken);
        if (doctor is null)
        {
            return;
        }

        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctor.Id, cancellationToken);
        if (patient != null)
        {
            if (doctor.LastSelectedPatientId == id)
            {
                doctor.LastSelectedPatientId = null;
            }

            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ProfileDto?> SelectAsync(long telegramUserId, Guid id, CancellationToken cancellationToken)
    {
        var doctor = await GetDoctorAsync(telegramUserId, cancellationToken);
        if (doctor is null)
        {
            return null;
        }

        doctor.Registration ??= new Domain.Entities.Registration();
        doctor.SpecializationCodes ??= new List<string>();
        doctor.SpecializationTitles ??= new List<string>();

        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctor.Id, cancellationToken);
        if (patient is null)
        {
            return null;
        }

        doctor.LastSelectedPatientId = id;
        await _db.SaveChangesAsync(cancellationToken);

        var codes = doctor.SpecializationCodes ?? new List<string>();
        var titles = doctor.SpecializationTitles ?? new List<string>();
        return new ProfileDto(
            doctor.Id,
            ToSpecializations(codes, titles),
            doctor.TelegramUserId,
            doctor.Registration?.Nickname,
            doctor.LastSelectedPatientId,
            patient.Nickname);
    }

    private async Task<Domain.Entities.Doctor?> GetDoctorAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return null;
        }

        return await _db.Doctors
            .Include(d => d.Registration)
            .FirstOrDefaultAsync(d => d.TelegramUserId == telegramUserId, cancellationToken);
    }

    private static PatientDto ToDto(Domain.Entities.Patient patient) =>
        new(patient.Id, patient.Sex, patient.AgeYears, patient.Nickname, patient.Allergies, patient.ChronicConditions,
            patient.Tags, patient.Status, patient.Notes, patient.LastInteractionAt);

    private static IReadOnlyCollection<SpecializationDto> ToSpecializations(
        IReadOnlyList<string> codes,
        IReadOnlyList<string> titles)
    {
        var count = Math.Min(codes.Count, titles.Count);
        var list = new List<SpecializationDto>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(new SpecializationDto(codes[i], titles[i]));
        }

        return list.AsReadOnly();
    }
}
