using MedAssist.Application.Abstractions;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Entities;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Persistence;

namespace MedAssist.Infrastructure.Services;

public class RegistrationService : IRegistrationService
{
    private readonly InMemoryDataStore _dataStore;
    private readonly ICurrentUserContext _currentUser;

    public RegistrationService(InMemoryDataStore dataStore, ICurrentUserContext currentUser)
    {
        _dataStore = dataStore;
        _currentUser = currentUser;
    }

    public Task<RegistrationDto> UpsertAsync(UpsertRegistrationRequest request, CancellationToken cancellationToken)
    {
        var doctor = EnsureDoctor();

        doctor.DisplayName = request.DisplayName;
        doctor.SpecializationCode = request.SpecializationCode;
        doctor.SpecializationTitle = request.SpecializationTitle;
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
        }

        TryCompleteRegistration(doctor);
        return Task.FromResult(ToDto(doctor));
    }

    public Task<RegistrationDto> GetAsync(CancellationToken cancellationToken)
    {
        var doctor = EnsureDoctor();
        return Task.FromResult(ToDto(doctor));
    }

    private Domain.Entities.Doctor EnsureDoctor() => _dataStore.GetOrCreateDoctor(_currentUser.GetCurrentUserId());

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
