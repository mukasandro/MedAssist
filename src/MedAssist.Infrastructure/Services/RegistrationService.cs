using System.Collections.Generic;
using System.Linq;
using MedAssist.Application.Exceptions;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Entities;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedAssist.Infrastructure.Services;

public class RegistrationService : IRegistrationService
{
    private readonly MedAssistDbContext _db;
    private readonly IReferenceService _referenceService;

    public RegistrationService(MedAssistDbContext db, IReferenceService referenceService)
    {
        _db = db;
        _referenceService = referenceService;
    }

    public async Task<RegistrationDto> UpsertAsync(UpsertRegistrationRequest request, CancellationToken cancellationToken)
    {
        var existing = await _db.Doctors.AsNoTracking()
            .AnyAsync(d => d.TelegramUserId == request.TelegramUserId, cancellationToken);
        if (existing)
        {
            throw new ConflictException("Doctor with this Telegram user id already exists.");
        }

        var doctor = await EnsureDoctorAsync(request.TelegramUserId, cancellationToken);

        if (request.SpecializationCodes is not null)
        {
            await ApplySpecializationCodesAsync(doctor, request.SpecializationCodes, cancellationToken);
            doctor.Registration.SpecializationCodes = doctor.SpecializationCodes.ToList();
            doctor.Registration.SpecializationTitles = doctor.SpecializationTitles.ToList();
        }
        doctor.TelegramUserId = request.TelegramUserId;
        doctor.Registration.Nickname = string.IsNullOrWhiteSpace(request.Nickname)
            ? null
            : request.Nickname.Trim();

        if (doctor.Registration.Status == RegistrationStatus.NotStarted)
        {
            doctor.Registration.Status = RegistrationStatus.InProgress;
            doctor.Registration.StartedAt = DateTimeOffset.UtcNow;
            doctor.RegisteredAt ??= DateTimeOffset.UtcNow;
        }

        TryCompleteRegistration(doctor);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(doctor);
    }

    public async Task<bool> UnregisterAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        var doctor = await _db.Doctors.Include(d => d.Registration)
            .FirstOrDefaultAsync(d => d.TelegramUserId == telegramUserId, cancellationToken);
        if (doctor is null)
        {
            return false;
        }

        _db.Doctors.Remove(doctor);

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<Domain.Entities.Doctor> EnsureDoctorAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        var doctor = await _db.Doctors.Include(d => d.Registration)
            .FirstOrDefaultAsync(d => d.TelegramUserId == telegramUserId, cancellationToken);

        if (doctor != null)
        {
            doctor.SpecializationCodes ??= new List<string>();
            doctor.Registration.SpecializationCodes ??= new List<string>();
            doctor.Registration.SpecializationTitles ??= new List<string>();
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
        doctor.Registration.SpecializationCodes ??= new List<string>();
        doctor.Registration.SpecializationTitles ??= new List<string>();
        return doctor;
    }

    private static RegistrationDto ToDto(Domain.Entities.Doctor doctor)
    {
        var reg = doctor.Registration;
        reg.SpecializationCodes ??= new List<string>();
        reg.SpecializationTitles ??= new List<string>();
        return new RegistrationDto(
            reg.Status,
            ToSpecializations(reg.SpecializationCodes, reg.SpecializationTitles),
            reg.Nickname,
            reg.StartedAt,
            doctor.TelegramUserId);
    }

    private static void TryCompleteRegistration(Domain.Entities.Doctor doctor)
    {
        var reg = doctor.Registration;
        reg.SpecializationCodes ??= new List<string>();
        if (reg.Status == RegistrationStatus.Completed)
        {
            return;
        }

        if (reg.SpecializationCodes.Count > 0)
        {
            reg.Status = RegistrationStatus.Completed;
        }
        else if (reg.Status == RegistrationStatus.NotStarted)
        {
            reg.Status = RegistrationStatus.InProgress;
            reg.StartedAt ??= DateTimeOffset.UtcNow;
        }
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
