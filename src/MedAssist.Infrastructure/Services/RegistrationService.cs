using MedAssist.Application.Abstractions;
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
    private readonly ICurrentUserContext _currentUser;

    public RegistrationService(MedAssistDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<RegistrationDto> UpsertAsync(UpsertRegistrationRequest request, CancellationToken cancellationToken)
    {
        var doctor = await EnsureDoctorAsync(cancellationToken);

        doctor.DisplayName = request.DisplayName;
        doctor.SpecializationCode = request.SpecializationCode;
        doctor.SpecializationTitle = request.SpecializationTitle;
        doctor.TelegramUsername = request.TelegramUsername;
        doctor.Degrees = request.Degrees;
        doctor.ExperienceYears = request.ExperienceYears;
        doctor.Languages = request.Languages;
        doctor.Bio = request.Bio;
        doctor.FocusAreas = request.FocusAreas;
        doctor.AcceptingNewPatients = request.AcceptingNewPatients;
        doctor.Location = request.Location;
        doctor.ContactPolicy = request.ContactPolicy;
        doctor.AvatarUrl = request.AvatarUrl;
        doctor.Registration.Specialization = request.SpecializationTitle;
        doctor.Registration.HumanInLoopConfirmed = request.HumanInLoopConfirmed;

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

    public async Task<RegistrationDto> GetAsync(CancellationToken cancellationToken)
    {
        var doctor = await EnsureDoctorAsync(cancellationToken);
        return ToDto(doctor);
    }

    private async Task<Domain.Entities.Doctor> EnsureDoctorAsync(CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var doctor = await _db.Doctors.Include(d => d.Registration)
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

        if (doctor != null)
        {
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
        return doctor;
    }

    private static RegistrationDto ToDto(Domain.Entities.Doctor doctor)
    {
        var reg = doctor.Registration;
        return new RegistrationDto(reg.Status, reg.Specialization, reg.HumanInLoopConfirmed, reg.StartedAt);
    }

    private static void TryCompleteRegistration(Domain.Entities.Doctor doctor)
    {
        var reg = doctor.Registration;
        if (reg.Status == RegistrationStatus.Completed)
        {
            return;
        }

        if (reg.HumanInLoopConfirmed && !string.IsNullOrWhiteSpace(reg.Specialization))
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
