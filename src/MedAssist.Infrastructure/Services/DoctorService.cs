using System.Collections.Generic;
using System.Linq;
using MedAssist.Application.DTOs;
using MedAssist.Application.Services;
using MedAssist.Application.Requests;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Bogus;

namespace MedAssist.Infrastructure.Services;

public class DoctorService : IDoctorService
{
    private readonly MedAssistDbContext _db;
    private static readonly Guid DefaultDoctorId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public DoctorService(MedAssistDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<DoctorPublicDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var doctors = await _db.Doctors.AsNoTracking().ToListAsync(cancellationToken);
        return doctors.Select(ToDto).ToList();
    }

    public async Task<DoctorPublicDto?> UpdateAsync(Guid id, UpdateDoctorRequest request, CancellationToken cancellationToken)
    {
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (doctor == null) return null;

        doctor.Registration ??= new Domain.Entities.Registration();
        doctor.DisplayName = request.DisplayName;
        doctor.SpecializationCodes = request.SpecializationCodes.ToList();
        doctor.SpecializationTitles = request.SpecializationTitles.ToList();
        doctor.Degrees = request.Degrees;
        doctor.ExperienceYears = request.ExperienceYears;
        doctor.Languages = request.Languages;
        doctor.Bio = request.Bio;
        doctor.FocusAreas = request.FocusAreas;
        doctor.AcceptingNewPatients = request.AcceptingNewPatients;
        doctor.Location = request.Location;
        doctor.ContactPolicy = request.ContactPolicy;
        doctor.AvatarUrl = request.AvatarUrl;
        doctor.Verified = request.Verified;
        doctor.Rating = request.Rating;
        doctor.Registration.SpecializationCodes = doctor.SpecializationCodes.ToList();
        doctor.Registration.SpecializationTitles = doctor.SpecializationTitles.ToList();

        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(doctor);
    }

    public async Task<DoctorPublicDto> CreateRandomAsync(CancellationToken cancellationToken)
    {
        await EnsureDefaultDoctorAsync(cancellationToken);

        var faker = new Faker("ru");
        var doctor = new Domain.Entities.Doctor
        {
            Id = Guid.NewGuid(),
            DisplayName = $"Д-р {faker.Name.LastName()} {faker.Name.FirstName()}",
            SpecializationCodes = new List<string> { "cardiology" },
            SpecializationTitles = new List<string> { "Кардиология" },
            Degrees = faker.PickRandom(new[] { "к.м.н.", "д.м.н.", null }),
            ExperienceYears = faker.Random.Int(3, 25),
            Languages = "ru,en",
            Bio = faker.Lorem.Sentence(6),
            FocusAreas = faker.PickRandom(new[] { "профилактика,гипертензия", "аритмии", "гипертония" }),
            TelegramUsername = faker.Internet.UserName().Replace(".", "_"),
            AcceptingNewPatients = faker.Random.Bool(),
            Location = faker.Address.City(),
            ContactPolicy = "Отвечаю в рабочие часы",
            AvatarUrl = null,
            Verified = faker.Random.Bool(0.7f),
            Rating = Math.Round(faker.Random.Double(4, 5), 1),
            Registration = new Domain.Entities.Registration
            {
                Status = Domain.Enums.RegistrationStatus.Completed,
                SpecializationCodes = new List<string> { "cardiology" },
                SpecializationTitles = new List<string> { "Кардиология" },
                HumanInLoopConfirmed = true,
                StartedAt = DateTimeOffset.UtcNow
            }
        };

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(doctor);
    }

    private async Task EnsureDefaultDoctorAsync(CancellationToken cancellationToken)
    {
        var exists = await _db.Doctors.AnyAsync(d => d.Id == DefaultDoctorId, cancellationToken);
        if (!exists)
        {
            _db.Doctors.Add(new Domain.Entities.Doctor
            {
                Id = DefaultDoctorId,
                DisplayName = "Д-р Тестовый",
                SpecializationCodes = new List<string> { "therapy" },
                SpecializationTitles = new List<string> { "Терапия" },
                TelegramUsername = "test_doctor",
                AcceptingNewPatients = true,
                Languages = "ru",
                Registration = new Domain.Entities.Registration
                {
                    Status = Domain.Enums.RegistrationStatus.Completed,
                    SpecializationCodes = new List<string> { "therapy" },
                    SpecializationTitles = new List<string> { "Терапия" },
                    HumanInLoopConfirmed = true,
                    StartedAt = DateTimeOffset.UtcNow
                }
            });
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private static DoctorPublicDto ToDto(Domain.Entities.Doctor d)
    {
        var codes = d.SpecializationCodes ?? new List<string>();
        var titles = d.SpecializationTitles ?? new List<string>();
        return new(d.Id, d.DisplayName, codes.AsReadOnly(), titles.AsReadOnly(), d.Degrees, d.ExperienceYears, d.Languages, d.Bio,
            d.FocusAreas, d.AcceptingNewPatients, d.Location, d.ContactPolicy, d.AvatarUrl, d.Verified, d.Rating);
    }
}
