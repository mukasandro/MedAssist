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
        var doctor = await EnsureDoctorAsync(telegramUserId, cancellationToken);
        return doctor is null ? null : ToDto(doctor);
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
        doctor.SpecializationCodes ??= new List<string>();
        doctor.SpecializationTitles ??= new List<string>();
        if (request.SpecializationCodes is not null)
        {
            doctor.SpecializationCodes = request.SpecializationCodes.ToList();
            doctor.Registration.SpecializationCodes = doctor.SpecializationCodes.ToList();
        }

        if (request.SpecializationTitles is not null)
        {
            doctor.SpecializationTitles = request.SpecializationTitles.ToList();
            doctor.Registration.SpecializationTitles = doctor.SpecializationTitles.ToList();
        }
        if (request.Nickname is not null)
        {
            doctor.Registration.Nickname = request.Nickname;
        }

        if (request.LastSelectedPatientId.HasValue)
        {
            doctor.LastSelectedPatientId = request.LastSelectedPatientId;
        }

        doctor.Registration.SpecializationCodes = doctor.SpecializationCodes.ToList();
        doctor.Registration.SpecializationTitles = doctor.SpecializationTitles.ToList();

        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(doctor);
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
        return ToDto(doctor);
    }

    private async Task<Domain.Entities.Doctor?> EnsureDoctorAsync(long telegramUserId, CancellationToken cancellationToken)
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

    private static ProfileDto ToDto(Domain.Entities.Doctor doctor)
    {
        doctor.Registration ??= new Registration();
        var codes = doctor.SpecializationCodes ?? new List<string>();
        var titles = doctor.SpecializationTitles ?? new List<string>();
        return new(
            doctor.Id,
            codes.AsReadOnly(),
            titles.AsReadOnly(),
            doctor.TelegramUserId,
            doctor.Registration.Nickname,
            doctor.LastSelectedPatientId,
            doctor.Verified);
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
}
