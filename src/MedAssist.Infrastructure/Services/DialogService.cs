using MedAssist.Application.Abstractions;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedAssist.Infrastructure.Services;

public class DialogService : IDialogService
{
    private readonly MedAssistDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public DialogService(MedAssistDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<DialogDto> CreateAsync(CreateDialogRequest request, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        if (request.PatientId.HasValue)
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Id == request.PatientId.Value && p.DoctorId == doctorId, cancellationToken);
            if (patient is null)
            {
                throw new InvalidOperationException("Patient not found for current doctor.");
            }
        }

        var dialog = new Domain.Entities.Dialog
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            PatientId = request.PatientId,
            Topic = request.Topic,
            Status = DialogStatus.Open,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Dialogs.Add(dialog);

        if (request.PatientId is { } patientId)
        {
            var patientToUpdate = await _db.Patients.FirstOrDefaultAsync(p => p.Id == patientId && p.DoctorId == doctorId, cancellationToken);
            if (patientToUpdate != null)
            {
                patientToUpdate.LastDialogId = dialog.Id;
                patientToUpdate.LastInteractionAt = dialog.CreatedAt;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(dialog);
    }

    public async Task<IReadOnlyCollection<DialogDto>> GetListAsync(Guid? patientId, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var dialogs = await _db.Dialogs
            .Where(d => d.DoctorId == doctorId && (!patientId.HasValue || d.PatientId == patientId))
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);

        return dialogs.Select(ToDto).ToList().AsReadOnly();
    }

    public async Task<DialogDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var dialog = await _db.Dialogs.FirstOrDefaultAsync(d => d.Id == id && d.DoctorId == doctorId, cancellationToken);
        return dialog is null ? null : ToDto(dialog);
    }

    public async Task<DialogDto?> CloseAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var dialog = await _db.Dialogs.FirstOrDefaultAsync(d => d.Id == id && d.DoctorId == doctorId, cancellationToken);
        if (dialog != null)
        {
            if (dialog.Status == DialogStatus.Open)
            {
                dialog.Status = DialogStatus.Closed;
                dialog.ClosedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
            }

            return ToDto(dialog);
        }

        return null;
    }

    private static DialogDto ToDto(Domain.Entities.Dialog dialog) =>
        new(dialog.Id, dialog.PatientId, dialog.Topic, dialog.Status, dialog.CreatedAt, dialog.ClosedAt);
}
