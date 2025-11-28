using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IMessageService
{
    Task<IReadOnlyCollection<MessageDto>> GetListAsync(Guid dialogId, CancellationToken cancellationToken);
    Task<MessageDto?> AddAsync(Guid dialogId, CreateMessageRequest request, CancellationToken cancellationToken);
}
