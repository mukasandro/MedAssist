using MedAssist.Application.Abstractions;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Persistence;

namespace MedAssist.Infrastructure.Services;

public class ConsentService : IConsentService
{
    private readonly InMemoryDataStore _dataStore;
    private readonly ICurrentUserContext _currentUser;

    public ConsentService(InMemoryDataStore dataStore, ICurrentUserContext currentUser)
    {
        _dataStore = dataStore;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyCollection<ConsentDto>> GetListAsync(CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var consents = _dataStore.Consents.Values
            .Where(c => c.DoctorId == doctorId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(ToDto)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<ConsentDto>>(consents);
    }

    public Task<ConsentDto> AddHumanInLoopAsync(CreateHumanInLoopConsentRequest request, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var consent = new Domain.Entities.Consent
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            Type = ConsentType.HumanInLoop,
            Accepted = request.Accepted,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dataStore.Consents[consent.Id] = consent;
        return Task.FromResult(ToDto(consent));
    }

    private static ConsentDto ToDto(Domain.Entities.Consent consent) =>
        new(consent.Id, consent.Type, consent.Accepted, consent.CreatedAt);
}
