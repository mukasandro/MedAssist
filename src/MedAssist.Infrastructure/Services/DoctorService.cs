using System.Collections.Generic;
using System.Linq;
using MedAssist.Application.DTOs;
using MedAssist.Application.Exceptions;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Bogus;

namespace MedAssist.Infrastructure.Services;

public class DoctorService : IDoctorService
{
    private readonly MedAssistDbContext _db;
    private readonly IReferenceService _referenceService;
    private static readonly Guid DefaultDoctorId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public DoctorService(MedAssistDbContext db, IReferenceService referenceService)
    {
        _db = db;
        _referenceService = referenceService;
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
        doctor.SpecializationCodes ??= new List<string>();
        doctor.SpecializationTitles ??= new List<string>();

        if (request.SpecializationCodes is not null)
        {
            await ApplySpecializationCodesAsync(doctor, request.SpecializationCodes, cancellationToken);
        }
        if (request.TelegramUserId.HasValue)
        {
            var telegramUserId = request.TelegramUserId.Value;
            var exists = await _db.Doctors
                .AsNoTracking()
                .AnyAsync(d => d.TelegramUserId == telegramUserId && d.Id != doctor.Id, cancellationToken);
            if (exists)
            {
                throw new ConflictException("Doctor with this Telegram user id already exists.");
            }

            doctor.TelegramUserId = telegramUserId;
        }
        if (request.Nickname is not null)
        {
            doctor.Registration.Nickname = string.IsNullOrWhiteSpace(request.Nickname)
                ? null
                : request.Nickname.Trim();
        }
        doctor.Verified = request.Verified;
        if (request.SpecializationCodes is not null)
        {
            doctor.Registration.SpecializationCodes = doctor.SpecializationCodes.ToList();
            doctor.Registration.SpecializationTitles = doctor.SpecializationTitles.ToList();
        }

        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(doctor);
    }

    public async Task<DoctorPublicDto?> UpdateSpecializationAsync(Guid id, UpdateSpecializationRequest request, CancellationToken cancellationToken)
    {
        var doctor = await _db.Doctors.Include(d => d.Registration)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (doctor == null) return null;

        var specialization = await GetSpecializationAsync(request.Code, cancellationToken);
        if (specialization is null)
        {
            throw new InvalidOperationException("Specialization code not found.");
        }

        doctor.Registration ??= new Domain.Entities.Registration();
        doctor.SpecializationCodes = new List<string> { specialization.Code };
        doctor.SpecializationTitles = new List<string> { specialization.Title };
        doctor.Registration.SpecializationCodes = doctor.SpecializationCodes.ToList();
        doctor.Registration.SpecializationTitles = doctor.SpecializationTitles.ToList();

        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(doctor);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (doctor == null) return;

        var patients = _db.Patients.Where(p => p.DoctorId == id);
        _db.Patients.RemoveRange(patients);

        _db.Doctors.Remove(doctor);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<DoctorPublicDto> CreateRandomAsync(CancellationToken cancellationToken)
    {
        await EnsureDefaultDoctorAsync(cancellationToken);

        var faker = new Faker("ru");
        var doctor = new Domain.Entities.Doctor
        {
            Id = Guid.NewGuid(),
            SpecializationCodes = new List<string> { "cardiology" },
            SpecializationTitles = new List<string> { "Кардиология" },
            TelegramUserId = faker.Random.Long(100000000, 9999999999),
            Verified = faker.Random.Bool(0.7f),
            Registration = new Domain.Entities.Registration
            {
                Status = Domain.Enums.RegistrationStatus.Completed,
                SpecializationCodes = new List<string> { "cardiology" },
                SpecializationTitles = new List<string> { "Кардиология" },
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
                SpecializationCodes = new List<string> { "therapy" },
                SpecializationTitles = new List<string> { "Терапия" },
                TelegramUserId = 1000000001,
                Registration = new Domain.Entities.Registration
                {
                    Status = Domain.Enums.RegistrationStatus.Completed,
                    SpecializationCodes = new List<string> { "therapy" },
                    SpecializationTitles = new List<string> { "Терапия" },
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
        return new(d.Id, ToSpecializations(codes, titles), d.TelegramUserId, d.Registration?.Nickname, d.Verified);
    }

    private async Task<SpecializationDto?> GetSpecializationAsync(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var specializations = await _referenceService.GetSpecializationsAsync(cancellationToken);
        return specializations.FirstOrDefault(s =>
            string.Equals(s.Code, code.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyCollection<SpecializationDto> ToSpecializations(
        IReadOnlyList<string> codes,
        IReadOnlyList<string> titles)
    {
        var count = Math.Min(codes.Count, titles.Count);
        var list = new List<SpecializationDto>(count);
        for (var i = 0; i < count; i++)
        {
            list.Add(new SpecializationDto(codes[i], titles[i]));
        }

        return list.AsReadOnly();
    }

    private async Task ApplySpecializationCodesAsync(
        Domain.Entities.Doctor doctor,
        IReadOnlyCollection<string> codes,
        CancellationToken cancellationToken)
    {
        var normalized = codes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .ToList();

        if (normalized.Count == 0)
        {
            doctor.SpecializationCodes = new List<string>();
            doctor.SpecializationTitles = new List<string>();
            return;
        }

        var known = await _referenceService.GetSpecializationsAsync(cancellationToken);
        var map = known.ToDictionary(s => s.Code, s => s.Title, StringComparer.OrdinalIgnoreCase);

        var titles = new List<string>(normalized.Count);
        foreach (var code in normalized)
        {
            if (!map.TryGetValue(code, out var title))
            {
                throw new InvalidOperationException($"Specialization code not found: {code}");
            }

            titles.Add(title);
        }

        doctor.SpecializationCodes = normalized;
        doctor.SpecializationTitles = titles;
    }
}
