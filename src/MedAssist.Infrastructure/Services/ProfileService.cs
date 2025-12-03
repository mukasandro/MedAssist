using System.Collections.Generic;
using System.Linq;
using MedAssist.Application.Abstractions;
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
    private readonly ICurrentUserContext _currentUser;

    public ProfileService(MedAssistDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProfileDto> GetAsync(CancellationToken cancellationToken)
    {
        var doctor = await EnsureDoctorAsync(cancellationToken);
        return ToDto(doctor);
    }

    public async Task<ProfileDto> UpdateAsync(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var doctor = await EnsureDoctorAsync(cancellationToken);
        doctor.Registration ??= new Registration();
        doctor.SpecializationCodes ??= new List<string>();
        doctor.SpecializationTitles ??= new List<string>();
        doctor.DisplayName = request.DisplayName ?? doctor.DisplayName;
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
        doctor.Degrees = request.Degrees ?? doctor.Degrees;
        doctor.ExperienceYears = request.ExperienceYears ?? doctor.ExperienceYears;
        doctor.Languages = request.Languages ?? doctor.Languages;
        doctor.Bio = request.Bio ?? doctor.Bio;
        doctor.FocusAreas = request.FocusAreas ?? doctor.FocusAreas;
        doctor.AcceptingNewPatients = request.AcceptingNewPatients ?? doctor.AcceptingNewPatients;
        doctor.Location = request.Location ?? doctor.Location;
        doctor.ContactPolicy = request.ContactPolicy ?? doctor.ContactPolicy;
        doctor.AvatarUrl = request.AvatarUrl ?? doctor.AvatarUrl;

        if (request.LastSelectedPatientId.HasValue)
        {
            doctor.LastSelectedPatientId = request.LastSelectedPatientId;
        }

        doctor.Registration.SpecializationCodes = doctor.SpecializationCodes.ToList();
        doctor.Registration.SpecializationTitles = doctor.SpecializationTitles.ToList();

        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(doctor);
    }

    private async Task<Domain.Entities.Doctor> EnsureDoctorAsync(CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var doctor = await _db.Doctors.Include(d => d.Registration)
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

        if (doctor != null)
        {
            doctor.SpecializationCodes ??= new List<string>();
            doctor.SpecializationTitles ??= new List<string>();
            return doctor;
        }

        doctor = new Domain.Entities.Doctor
        {
            Id = doctorId,
            Registration = new Registration
            {
                Status = RegistrationStatus.NotStarted
            },
            AcceptingNewPatients = true
        };

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync(cancellationToken);
        doctor.SpecializationCodes ??= new List<string>();
        doctor.SpecializationTitles ??= new List<string>();
        return doctor;
    }

    private static ProfileDto ToDto(Domain.Entities.Doctor doctor)
    {
        var codes = doctor.SpecializationCodes ?? new List<string>();
        var titles = doctor.SpecializationTitles ?? new List<string>();
        return new(
            doctor.Id,
            doctor.DisplayName,
            codes.AsReadOnly(),
            titles.AsReadOnly(),
            doctor.TelegramUsername,
            doctor.Degrees,
            doctor.ExperienceYears,
            doctor.Languages,
            doctor.Bio,
            doctor.FocusAreas,
            doctor.AcceptingNewPatients,
            doctor.Location,
            doctor.ContactPolicy,
            doctor.AvatarUrl,
            doctor.RegistrationStatus,
            doctor.LastSelectedPatientId,
            doctor.Verified,
            doctor.Rating);
    }
}
