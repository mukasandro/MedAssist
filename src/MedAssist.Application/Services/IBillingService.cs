using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IBillingService
{
    Task<DoctorTokenBalanceDto> TopUpAsync(TopUpDoctorTokensRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<BillingTokenLedgerDto>> GetHistoryAsync(
        long? telegramUserId,
        int take,
        CancellationToken cancellationToken);
}
