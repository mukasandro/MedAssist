using System.Collections.Concurrent;
using MedAssist.Domain.Entities;
using MedAssist.Domain.Enums;

namespace MedAssist.Infrastructure.Persistence;

public class InMemoryDataStore
{
    public ConcurrentDictionary<Guid, Doctor> Doctors { get; } = new();
    public ConcurrentDictionary<Guid, Patient> Patients { get; } = new();
    public ConcurrentDictionary<Guid, Dialog> Dialogs { get; } = new();
    public ConcurrentDictionary<Guid, Message> Messages { get; } = new();
    public ConcurrentDictionary<Guid, Consent> Consents { get; } = new();

    public Doctor GetOrCreateDoctor(Guid doctorId)
    {
        return Doctors.GetOrAdd(doctorId, id => new Doctor
        {
            Id = id,
            Registration = new Registration
            {
                Status = RegistrationStatus.NotStarted
            }
        });
    }
}
