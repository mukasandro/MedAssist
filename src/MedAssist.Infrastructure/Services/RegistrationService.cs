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

public class RegistrationService : IRegistrationService
{
    private readonly MedAssistDbContext _db;

    public RegistrationService(MedAssistDbContext db)
    {
        _db = db;
    }

    public async Task<RegistrationDto> UpsertAsync(UpsertRegistrationRequest request, CancellationToken cancellationToken)
    {
        var doctor = await EnsureDoctorAsync(request.TelegramUserId, cancellationToken);

        doctor.SpecializationCodes = request.SpecializationCodes.ToList();
        doctor.TelegramUserId = request.TelegramUserId;
        doctor.Registration.SpecializationCodes = doctor.SpecializationCodes.ToList();
        doctor.Registration.Nickname = request.Nickname;
        doctor.Registration.Confirmed = request.Confirmed;

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

    public async Task<RegistrationDto?> UnregisterAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        var doctor = await _db.Doctors.Include(d => d.Registration)
            .FirstOrDefaultAsync(d => d.TelegramUserId == telegramUserId, cancellationToken);
        if (doctor is null)
        {
            return null;
        }

        doctor.Registration ??= new Registration();
        doctor.Registration.Status = RegistrationStatus.NotStarted;
        doctor.Registration.SpecializationCodes = new List<string>();
        doctor.Registration.SpecializationTitles = new List<string>();
        doctor.Registration.Nickname = null;
        doctor.Registration.Confirmed = false;
        doctor.Registration.StartedAt = null;
        doctor.RegisteredAt = null;

        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(doctor);
    }

    private async Task<Domain.Entities.Doctor> EnsureDoctorAsync(long telegramUserId, CancellationToken cancellationToken)
    {
        var doctor = await _db.Doctors.Include(d => d.Registration)
            .FirstOrDefaultAsync(d => d.TelegramUserId == telegramUserId, cancellationToken);

        if (doctor != null)
        {
            doctor.SpecializationCodes ??= new List<string>();
            doctor.Registration.SpecializationCodes ??= new List<string>();
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
        return doctor;
    }

    private static RegistrationDto ToDto(Domain.Entities.Doctor doctor)
    {
        var reg = doctor.Registration;
        reg.SpecializationCodes ??= new List<string>();
        return new RegistrationDto(
            reg.Status,
            reg.SpecializationCodes.AsReadOnly(),
            reg.Nickname,
            reg.Confirmed,
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

        if (reg.Confirmed && reg.SpecializationCodes.Count > 0)
        {
            reg.Status = RegistrationStatus.Completed;
        }
        else if (reg.Status == RegistrationStatus.NotStarted)
        {
            reg.Status = RegistrationStatus.InProgress;
            reg.StartedAt ??= DateTimeOffset.UtcNow;
        }
    }
}
