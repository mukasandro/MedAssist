using MedAssist.Application.Abstractions;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Persistence;

namespace MedAssist.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly InMemoryDataStore _dataStore;
    private readonly ICurrentUserContext _currentUser;

    public PatientService(InMemoryDataStore dataStore, ICurrentUserContext currentUser)
    {
        _dataStore = dataStore;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyCollection<PatientDto>> GetListAsync(CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var patients = _dataStore.Patients.Values
            .Where(p => p.DoctorId == doctorId)
            .OrderByDescending(p => p.LastInteractionAt ?? p.CreatedAt)
            .ThenBy(p => p.FullName)
            .Select(ToDto)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<PatientDto>>(patients);
    }

    public Task<PatientDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        if (_dataStore.Patients.TryGetValue(id, out var patient) && patient.DoctorId == doctorId)
        {
            return Task.FromResult<PatientDto?>(ToDto(patient));
        }

        return Task.FromResult<PatientDto?>(null);
    }

    public Task<PatientDto> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken)
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

        _dataStore.Patients[patient.Id] = patient;
        return Task.FromResult(ToDto(patient));
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        if (_dataStore.Patients.TryGetValue(id, out var patient) && patient.DoctorId == doctorId)
        {
            _dataStore.Patients.TryRemove(id, out _);
        }

        return Task.CompletedTask;
    }

    public Task<ProfileDto> SelectAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctor = _dataStore.GetOrCreateDoctor(_currentUser.GetCurrentUserId());
        if (_dataStore.Patients.TryGetValue(id, out var patient) && patient.DoctorId == doctor.Id)
        {
            doctor.LastSelectedPatientId = id;
        }

        return Task.FromResult(new ProfileDto(
            doctor.Id,
            doctor.DisplayName,
            doctor.SpecializationCode,
            doctor.SpecializationTitle,
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
            doctor.Rating));
    }

    private static PatientDto ToDto(Domain.Entities.Patient patient) =>
        new(patient.Id, patient.FullName, patient.BirthDate, patient.Sex, patient.Phone, patient.Email, patient.Allergies,
            patient.ChronicConditions, patient.Tags, patient.Status, patient.Notes, patient.CreatedAt, patient.UpdatedAt,
            patient.LastDialogId, patient.LastSummary, patient.LastInteractionAt);
}
