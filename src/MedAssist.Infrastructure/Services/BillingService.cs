using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedAssist.Infrastructure.Services;

public class BillingService : IBillingService
{
    private readonly MedAssistDbContext _db;

    public BillingService(MedAssistDbContext db)
    {
        _db = db;
    }

    public async Task<DoctorTokenBalanceDto> TopUpAsync(TopUpDoctorTokensRequest request, CancellationToken cancellationToken)
    {
        var doctor = await _db.Doctors
            .FirstOrDefaultAsync(x => x.TelegramUserId == request.TelegramUserId, cancellationToken);
        if (doctor is null || !doctor.TelegramUserId.HasValue)
        {
            throw new KeyNotFoundException("Doctor with provided telegramUserId was not found.");
        }

        checked
        {
            doctor.TokenBalance += request.Tokens;
        }

        _db.BillingTokenLedgers.Add(new Domain.Entities.BillingTokenLedger
        {
            Id = Guid.NewGuid(),
            DoctorId = doctor.Id,
            TelegramUserId = doctor.TelegramUserId.Value,
            Delta = request.Tokens,
            BalanceAfter = doctor.TokenBalance,
            Reason = "TopUp",
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);

        return new DoctorTokenBalanceDto(
            doctor.Id,
            doctor.TelegramUserId.Value,
            doctor.TokenBalance);
    }

    public async Task<IReadOnlyCollection<BillingTokenLedgerDto>> GetHistoryAsync(
        long? telegramUserId,
        int take,
        CancellationToken cancellationToken)
    {
        var normalizedTake = Math.Clamp(take, 1, 500);
        var query = _db.BillingTokenLedgers
            .AsNoTracking()
            .AsQueryable();

        if (telegramUserId.HasValue)
        {
            query = query.Where(x => x.TelegramUserId == telegramUserId.Value);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(normalizedTake)
            .Select(x => new BillingTokenLedgerDto(
                x.Id,
                x.DoctorId,
                x.TelegramUserId,
                x.Delta,
                x.BalanceAfter,
                x.Reason,
                x.ConversationId,
                x.ChatTurnId,
                x.RequestId,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return items.AsReadOnly();
    }
}
