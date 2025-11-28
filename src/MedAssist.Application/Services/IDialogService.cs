using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;

namespace MedAssist.Application.Services;

public interface IDialogService
{
    Task<DialogDto> CreateAsync(CreateDialogRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DialogDto>> GetListAsync(Guid? patientId, CancellationToken cancellationToken);
    Task<DialogDto?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<DialogDto?> CloseAsync(Guid id, CancellationToken cancellationToken);
}
