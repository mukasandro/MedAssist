using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedAssist.Infrastructure.Services;

public class StaticContentService : IStaticContentService
{
    private readonly MedAssistDbContext _db;

    public StaticContentService(MedAssistDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<StaticContentAdminDto>> GetListAsync(CancellationToken cancellationToken)
    {
        var items = await _db.StaticContents
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(ToAdminDto).ToList().AsReadOnly();
    }

    public async Task<StaticContentAdminDto> CreateAsync(CreateStaticContentRequest request, CancellationToken cancellationToken)
    {
        var code = NormalizeCode(request.Code);
        if (await _db.StaticContents.AnyAsync(x => x.Code == code, cancellationToken))
        {
            throw new InvalidOperationException("Static content code already exists.");
        }

        var entity = new Domain.Entities.StaticContent
        {
            Id = Guid.NewGuid(),
            Code = code,
            Title = NormalizeTitle(request.Title),
            Value = request.Value,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.StaticContents.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ToAdminDto(entity);
    }

    public async Task<StaticContentAdminDto?> UpdateAsync(Guid id, UpdateStaticContentRequest request, CancellationToken cancellationToken)
    {
        var entity = await _db.StaticContents.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var code = NormalizeCode(request.Code);
        var exists = await _db.StaticContents.AnyAsync(x => x.Code == code && x.Id != id, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Static content code already exists.");
        }

        entity.Code = code;
        entity.Title = NormalizeTitle(request.Title);
        entity.Value = request.Value;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return ToAdminDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.StaticContents.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity != null)
        {
            _db.StaticContents.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<StaticContentDto?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var normalizedCode = NormalizeCode(code);
        var entity = await _db.StaticContents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);

        return entity is null ? null : ToBotDto(entity);
    }

    private static string NormalizeCode(string code) =>
        code.Trim().ToLowerInvariant();

    private static string? NormalizeTitle(string? title) =>
        string.IsNullOrWhiteSpace(title) ? null : title.Trim();

    private static StaticContentAdminDto ToAdminDto(Domain.Entities.StaticContent entity) =>
        new(entity.Id, entity.Code, entity.Title, entity.Value, entity.UpdatedAt);

    private static StaticContentDto ToBotDto(Domain.Entities.StaticContent entity) =>
        new(entity.Code, entity.Value);
}
