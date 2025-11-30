using MedAssist.Application.Abstractions;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedAssist.Infrastructure.Services;

public class ConsentService : IConsentService
{
    private readonly MedAssistDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public ConsentService(MedAssistDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<ConsentDto>> GetListAsync(CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var consents = await _db.Consents
            .Where(c => c.DoctorId == doctorId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return consents.Select(ToDto).ToList().AsReadOnly();
    }

    public async Task<ConsentDto> AddHumanInLoopAsync(CreateHumanInLoopConsentRequest request, CancellationToken cancellationToken)
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

        _db.Consents.Add(consent);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(consent);
    }

    private static ConsentDto ToDto(Domain.Entities.Consent consent) =>
        new(consent.Id, consent.Type, consent.Accepted, consent.CreatedAt);
}
