using MedAssist.Application.Abstractions;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Persistence;

namespace MedAssist.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly InMemoryDataStore _dataStore;
    private readonly ICurrentUserContext _currentUser;

    public MessageService(InMemoryDataStore dataStore, ICurrentUserContext currentUser)
    {
        _dataStore = dataStore;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyCollection<MessageDto>> GetListAsync(Guid dialogId, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        if (!_dataStore.Dialogs.TryGetValue(dialogId, out var dialog) || dialog.DoctorId != doctorId)
        {
            return Task.FromResult<IReadOnlyCollection<MessageDto>>(Array.Empty<MessageDto>());
        }

        var messages = _dataStore.Messages.Values
            .Where(m => m.DialogId == dialogId)
            .OrderBy(m => m.CreatedAt)
            .Select(ToDto)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<MessageDto>>(messages);
    }

    public Task<MessageDto?> AddAsync(Guid dialogId, CreateMessageRequest request, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        if (!_dataStore.Dialogs.TryGetValue(dialogId, out var dialog) || dialog.DoctorId != doctorId)
        {
            return Task.FromResult<MessageDto?>(null);
        }

        if (!Enum.TryParse<MessageRole>(request.Role, true, out var role))
        {
            return Task.FromResult<MessageDto?>(null);
        }

        var message = new Domain.Entities.Message
        {
            Id = Guid.NewGuid(),
            DialogId = dialogId,
            Role = role,
            Content = request.Content,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dataStore.Messages[message.Id] = message;

        if (dialog.PatientId is { } patientId && _dataStore.Patients.TryGetValue(patientId, out var patient))
        {
            patient.LastInteractionAt = message.CreatedAt;
            patient.LastDialogId = dialog.Id;
            patient.LastSummary = message.Content.Length > 180 ? message.Content[..180] : message.Content;
        }

        return Task.FromResult<MessageDto?>(ToDto(message));
    }

    private static MessageDto ToDto(Domain.Entities.Message message) =>
        new(message.Id, message.DialogId, message.Role, message.Content, message.CreatedAt);
}
