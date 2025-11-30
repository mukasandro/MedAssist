using MedAssist.Application.Abstractions;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedAssist.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly MedAssistDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public MessageService(MedAssistDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<MessageDto>> GetListAsync(Guid dialogId, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var dialog = await _db.Dialogs.FirstOrDefaultAsync(d => d.Id == dialogId && d.DoctorId == doctorId, cancellationToken);
        if (dialog is null)
        {
            return Array.Empty<MessageDto>();
        }

        var messages = await _db.Messages
            .Where(m => m.DialogId == dialogId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return messages.Select(ToDto).ToList().AsReadOnly();
    }

    public async Task<MessageDto?> AddAsync(Guid dialogId, CreateMessageRequest request, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var dialog = await _db.Dialogs.FirstOrDefaultAsync(d => d.Id == dialogId && d.DoctorId == doctorId, cancellationToken);
        if (dialog is null)
        {
            return null;
        }

        if (!Enum.TryParse<MessageRole>(request.Role, true, out var role))
        {
            return null;
        }

        var message = new Domain.Entities.Message
        {
            Id = Guid.NewGuid(),
            DialogId = dialogId,
            Role = role,
            Content = request.Content,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Messages.Add(message);

        if (dialog.PatientId is { } patientId)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == patientId && p.DoctorId == doctorId, cancellationToken);
            if (patient != null)
            {
                patient.LastInteractionAt = message.CreatedAt;
                patient.LastDialogId = dialog.Id;
                patient.LastSummary = message.Content.Length > 180 ? message.Content[..180] : message.Content;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(message);
    }

    private static MessageDto ToDto(Domain.Entities.Message message) =>
        new(message.Id, message.DialogId, message.Role, message.Content, message.CreatedAt);
}
