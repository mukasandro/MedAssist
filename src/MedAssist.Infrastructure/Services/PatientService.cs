using System.Collections.Generic;
using MedAssist.Application.Abstractions;
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
    private readonly ICurrentUserContext _currentUser;

    public PatientService(MedAssistDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<PatientDto>> GetListAsync(CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var patients = await _db.Patients
            .Where(p => p.DoctorId == doctorId)
            .OrderByDescending(p => p.LastInteractionAt ?? p.CreatedAt)
            .ThenBy(p => p.FullName)
            .ToListAsync(cancellationToken);

        return patients.Select(ToDto).ToList().AsReadOnly();
    }

    public async Task<PatientDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctorId, cancellationToken);
        return patient is null ? null : ToDto(patient);
    }

    public async Task<PatientDto> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var patient = new Domain.Entities.Patient
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            FullName = request.FullName,
            BirthDate = request.BirthDate,
            Sex = request.Sex,
            Phone = request.Phone,
            Email = request.Email,
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

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctorId, cancellationToken);
        if (patient != null)
        {
            _db.Patients.Remove(patient);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ProfileDto> SelectAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var doctor = await _db.Doctors.Include(d => d.Registration).FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);
        if (doctor != null)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctorId, cancellationToken);
            if (patient != null)
            {
                doctor.LastSelectedPatientId = id;
                await _db.SaveChangesAsync(cancellationToken);
            }

            var codes = doctor.SpecializationCodes ?? new List<string>();
            var titles = doctor.SpecializationTitles ?? new List<string>();
            return new ProfileDto(
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

        throw new InvalidOperationException("Doctor not found");
    }

    private static PatientDto ToDto(Domain.Entities.Patient patient) =>
        new(patient.Id, patient.FullName, patient.BirthDate, patient.Sex, patient.Phone, patient.Email, patient.Allergies,
            patient.ChronicConditions, patient.Tags, patient.Status, patient.Notes, patient.CreatedAt, patient.UpdatedAt,
            patient.LastDialogId, patient.LastSummary, patient.LastInteractionAt);
}
