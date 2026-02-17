using System.Text.Json;
using MedAssist.Application.DTOs;
using MedAssist.Application.Requests;
using MedAssist.Application.Services;
using MedAssist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MedAssist.Infrastructure.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private const string AppSettingsKey = "app.settings";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly MedAssistDbContext _db;

    public SystemSettingsService(MedAssistDbContext db)
    {
        _db = db;
    }

    public async Task<SystemSettingsDto> GetAsync(CancellationToken cancellationToken)
    {
        var entity = await _db.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == AppSettingsKey, cancellationToken);

        if (entity is null)
        {
            return new SystemSettingsDto(null, DateTimeOffset.UtcNow);
        }

        var payload = Deserialize(entity.ValueJson);
        return new SystemSettingsDto(payload.LlmGatewayUrl, entity.UpdatedAt);
    }

    public async Task<SystemSettingsDto> UpdateAsync(UpdateSystemSettingsRequest request, CancellationToken cancellationToken)
    {
        var entity = await _db.SystemSettings
            .FirstOrDefaultAsync(x => x.Key == AppSettingsKey, cancellationToken);

        var normalizedUrl = NormalizeUrl(request.LlmGatewayUrl);
        if (entity is null)
        {
            entity = new Domain.Entities.SystemSetting
            {
                Id = Guid.NewGuid(),
                Key = AppSettingsKey,
                ValueJson = Serialize(new AppSettingsPayload(normalizedUrl)),
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.SystemSettings.Add(entity);
        }
        else
        {
            entity.ValueJson = Serialize(new AppSettingsPayload(normalizedUrl));
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new SystemSettingsDto(normalizedUrl, entity.UpdatedAt);
    }

    public async Task<string?> GetLlmGatewayUrlAsync(CancellationToken cancellationToken)
    {
        var settings = await GetAsync(cancellationToken);
        return settings.LlmGatewayUrl;
    }

    private static AppSettingsPayload Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new AppSettingsPayload(null);
        }

        try
        {
            var payload = JsonSerializer.Deserialize<AppSettingsPayload>(json, JsonOptions);
            return payload is null
                ? new AppSettingsPayload(null)
                : payload with { LlmGatewayUrl = NormalizeUrl(payload.LlmGatewayUrl) };
        }
        catch (JsonException)
        {
            return new AppSettingsPayload(null);
        }
    }

    private static string Serialize(AppSettingsPayload payload)
        => JsonSerializer.Serialize(payload, JsonOptions);

    private static string? NormalizeUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private sealed record AppSettingsPayload(string? LlmGatewayUrl);
}
