using System.Collections.Generic;
using System.Linq;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Entities;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedAssist.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly MedAssistDbContext _db;
    private readonly IReferenceService _referenceService;

    public ProfileService(
        MedAssistDbContext db,
        IReferenceService referenceService)
    {
        _db = db;
        _referenceService = referenceService;
    }

    public async Task<ProfileDto?> GetAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        var doctor = await GetDoctorAsync(telegramUserId, cancellationToken);
        return doctor is null ? null : await ToDtoAsync(doctor, cancellationToken);
    }

    public async Task<ProfileDto?> UpdateAsync(
        long telegramUserId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var doctor = await EnsureDoctorAsync(telegramUserId, cancellationToken);
        if (doctor is null)
        {
            return null;
        }

        doctor.Registration ??= new Registration();
        if (request.Nickname is not null)
        {
            doctor.Registration.Nickname = string.IsNullOrWhiteSpace(request.Nickname)
                ? null
                : request.Nickname.Trim();
        }

        await _db.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(doctor, cancellationToken);
    }

    public async Task<ProfileDto?> UpdateSpecializationAsync(
        long telegramUserId,
        UpdateSpecializationRequest request,
        CancellationToken cancellationToken)
    {
        var doctor = await EnsureDoctorAsync(telegramUserId, cancellationToken);
        if (doctor is null)
        {
            return null;
        }

        var specialization = await GetSpecializationAsync(request.Code, cancellationToken);
        if (specialization is null)
        {
            throw new InvalidOperationException("Specialization code not found.");
        }

        doctor.Registration ??= new Registration();
        doctor.SpecializationCodes = new List<string> { specialization.Code };
        doctor.SpecializationTitles = new List<string> { specialization.Title };
        doctor.Registration.SpecializationCodes = doctor.SpecializationCodes.ToList();
        doctor.Registration.SpecializationTitles = doctor.SpecializationTitles.ToList();

        await _db.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(doctor, cancellationToken);
    }

    public async Task<bool> SetActivePatientAsync(
        long telegramUserId,
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var doctor = await GetDoctorAsync(telegramUserId, cancellationToken);
        if (doctor is null)
        {
            return false;
        }

        var patientExists = await _db.Patients
            .AsNoTracking()
            .AnyAsync(p => p.Id == patientId && p.DoctorId == doctor.Id, cancellationToken);
        if (!patientExists)
        {
            return false;
        }

        doctor.LastSelectedPatientId = patientId;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ClearActivePatientAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        var doctor = await GetDoctorAsync(telegramUserId, cancellationToken);
        if (doctor is null)
        {
            return false;
        }

        doctor.LastSelectedPatientId = null;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<Domain.Entities.Doctor?> EnsureDoctorAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return null;
        }

        var doctor = await GetDoctorAsync(telegramUserId, cancellationToken);
        if (doctor != null)
        {
            return doctor;
        }

        doctor = new Domain.Entities.Doctor
        {
            Id = Guid.NewGuid(),
            TelegramUserId = telegramUserId,
            Registration = new Registration
            {
                Status = RegistrationStatus.NotStarted
            }
        };

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync(cancellationToken);
        doctor.SpecializationCodes ??= new List<string>();
        doctor.SpecializationTitles ??= new List<string>();
        return doctor;
    }

    private async Task<Domain.Entities.Doctor?> GetDoctorAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        if (telegramUserId <= 0)
        {
            return null;
        }

        var doctor = await _db.Doctors.Include(d => d.Registration)
            .FirstOrDefaultAsync(d => d.TelegramUserId == telegramUserId, cancellationToken);

        if (doctor != null)
        {
            doctor.SpecializationCodes ??= new List<string>();
            doctor.SpecializationTitles ??= new List<string>();
            return doctor;
        }

        return null;
    }

    private async Task<ProfileDto> ToDtoAsync(Domain.Entities.Doctor doctor, CancellationToken cancellationToken)
    {
        doctor.Registration ??= new Registration();
        var codes = doctor.SpecializationCodes ?? new List<string>();
        var titles = doctor.SpecializationTitles ?? new List<string>();

        string? lastSelectedPatientNickname = null;
        if (doctor.LastSelectedPatientId.HasValue)
        {
            lastSelectedPatientNickname = await _db.Patients
                .AsNoTracking()
                .Where(p => p.Id == doctor.LastSelectedPatientId.Value && p.DoctorId == doctor.Id)
                .Select(p => p.Nickname)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new(
            doctor.Id,
            ToSpecializations(codes, titles),
            doctor.TelegramUserId,
            doctor.Registration.Nickname,
            doctor.LastSelectedPatientId,
            lastSelectedPatientNickname);
    }

    private async Task<SpecializationDto?> GetSpecializationAsync(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var specializations = await _referenceService.GetSpecializationsAsync(cancellationToken);
        return specializations.FirstOrDefault(s =>
            string.Equals(s.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
    }

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

    private async Task ApplySpecializationCodesAsync(
        Domain.Entities.Doctor doctor,
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken)
    {
        var normalized = codes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .ToList();

        if (normalized.Count == 0)
        {
            doctor.SpecializationCodes = new List<string>();
            doctor.SpecializationTitles = new List<string>();
            return;
        }

        var known = await _referenceService.GetSpecializationsAsync(cancellationToken);
        var map = known.ToDictionary(s => s.Code, s => s.Title, StringComparer.OrdinalIgnoreCase);

        var titles = new List<string>(normalized.Count);
        foreach (var code in normalized)
        {
            if (!map.TryGetValue(code, out var title))
            {
                throw new InvalidOperationException($"Specialization code not found: {code}");
            }

            titles.Add(title);
        }

        doctor.SpecializationCodes = normalized;
        doctor.SpecializationTitles = titles;
    }
}
