using MedAssist.Application.Abstractions;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Domain.Enums;
using MedAssist.Infrastructure.Persistence;

namespace MedAssist.Infrastructure.Services;

public class DialogService : IDialogService
{
    private readonly InMemoryDataStore _dataStore;
    private readonly ICurrentUserContext _currentUser;

    public DialogService(InMemoryDataStore dataStore, ICurrentUserContext currentUser)
    {
        _dataStore = dataStore;
        _currentUser = currentUser;
    }

    public Task<DialogDto> CreateAsync(CreateDialogRequest request, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        if (request.PatientId.HasValue)
        {
            if (!_dataStore.Patients.TryGetValue(request.PatientId.Value, out var patient) || patient.DoctorId != doctorId)
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

        _dataStore.Dialogs[dialog.Id] = dialog;

        if (request.PatientId is { } patientId && _dataStore.Patients.TryGetValue(patientId, out var patientToUpdate))
        {
            patientToUpdate.LastDialogId = dialog.Id;
            patientToUpdate.LastInteractionAt = dialog.CreatedAt;
        }

        return Task.FromResult(ToDto(dialog));
    }

    public Task<IReadOnlyCollection<DialogDto>> GetListAsync(Guid? patientId, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        var dialogs = _dataStore.Dialogs.Values
            .Where(d => d.DoctorId == doctorId && (!patientId.HasValue || d.PatientId == patientId))
            .OrderByDescending(d => d.CreatedAt)
            .Select(ToDto)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyCollection<DialogDto>>(dialogs);
    }

    public Task<DialogDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        if (_dataStore.Dialogs.TryGetValue(id, out var dialog) && dialog.DoctorId == doctorId)
        {
            return Task.FromResult<DialogDto?>(ToDto(dialog));
        }

        return Task.FromResult<DialogDto?>(null);
    }

    public Task<DialogDto?> CloseAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctorId = _currentUser.GetCurrentUserId();
        if (_dataStore.Dialogs.TryGetValue(id, out var dialog) && dialog.DoctorId == doctorId)
        {
            if (dialog.Status == DialogStatus.Open)
            {
                dialog.Status = DialogStatus.Closed;
                dialog.ClosedAt = DateTimeOffset.UtcNow;
            }

            return Task.FromResult<DialogDto?>(ToDto(dialog));
        }

        return Task.FromResult<DialogDto?>(null);
    }

    private static DialogDto ToDto(Domain.Entities.Dialog dialog) =>
        new(dialog.Id, dialog.PatientId, dialog.Topic, dialog.Status, dialog.CreatedAt, dialog.ClosedAt);
}
