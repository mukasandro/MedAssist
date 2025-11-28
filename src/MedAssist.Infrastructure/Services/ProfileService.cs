using MedAssist.Application.Abstractions;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Infrastructure.Persistence;

namespace MedAssist.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly InMemoryDataStore _dataStore;
    private readonly ICurrentUserContext _currentUser;

    public ProfileService(InMemoryDataStore dataStore, ICurrentUserContext currentUser)
    {
        _dataStore = dataStore;
        _currentUser = currentUser;
    }

    public Task<ProfileDto> GetAsync(CancellationToken cancellationToken)
    {
        var doctor = EnsureDoctor();
        return Task.FromResult(ToDto(doctor));
    }

    public Task<ProfileDto> UpdateAsync(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var doctor = EnsureDoctor();
        doctor.DisplayName = request.DisplayName ?? doctor.DisplayName;
        doctor.SpecializationCode = request.SpecializationCode ?? doctor.SpecializationCode;
        doctor.SpecializationTitle = request.SpecializationTitle ?? doctor.SpecializationTitle;
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

        return Task.FromResult(ToDto(doctor));
    }

    private Domain.Entities.Doctor EnsureDoctor() => _dataStore.GetOrCreateDoctor(_currentUser.GetCurrentUserId());

    private static ProfileDto ToDto(Domain.Entities.Doctor doctor) =>
        new(
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
            doctor.Rating);
}
