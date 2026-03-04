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
    private const int DefaultEnrichChatHistoryDepth = 5;
    private const string DefaultEnrichServiceUrl = "https://enrich.muk.i234.me";
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
            return new SystemSettingsDto(null, DefaultEnrichServiceUrl, DefaultEnrichChatHistoryDepth, DateTimeOffset.UtcNow);
        }

        var payload = Deserialize(entity.ValueJson);
        return new SystemSettingsDto(
            payload.LlmGatewayUrl,
            NormalizeRequiredUrl(payload.EnrichServiceUrl, DefaultEnrichServiceUrl),
            NormalizeEnrichChatHistoryDepth(payload.EnrichChatHistoryDepth),
            entity.UpdatedAt);
    }

    public async Task<SystemSettingsDto> UpdateAsync(UpdateSystemSettingsRequest request, CancellationToken cancellationToken)
    {
        var entity = await _db.SystemSettings
            .FirstOrDefaultAsync(x => x.Key == AppSettingsKey, cancellationToken);

        var normalizedUrl = NormalizeUrl(request.LlmGatewayUrl);
        var normalizedEnrichServiceUrl = NormalizeRequiredUrl(request.EnrichServiceUrl, DefaultEnrichServiceUrl);
        var normalizedDepth = NormalizeEnrichChatHistoryDepth(request.EnrichChatHistoryDepth);
        if (entity is null)
        {
            entity = new Domain.Entities.SystemSetting
            {
                Id = Guid.NewGuid(),
                Key = AppSettingsKey,
                ValueJson = Serialize(new AppSettingsPayload(normalizedUrl, normalizedEnrichServiceUrl, normalizedDepth)),
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _db.SystemSettings.Add(entity);
        }
        else
        {
            entity.ValueJson = Serialize(new AppSettingsPayload(normalizedUrl, normalizedEnrichServiceUrl, normalizedDepth));
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new SystemSettingsDto(normalizedUrl, normalizedEnrichServiceUrl, normalizedDepth, entity.UpdatedAt);
    }

    public async Task<string?> GetLlmGatewayUrlAsync(CancellationToken cancellationToken)
    {
        var settings = await GetAsync(cancellationToken);
        return settings.LlmGatewayUrl;
    }

    public async Task<int> GetEnrichChatHistoryDepthAsync(CancellationToken cancellationToken)
    {
        var settings = await GetAsync(cancellationToken);
        return settings.EnrichChatHistoryDepth;
    }

    public async Task<string> GetEnrichServiceUrlAsync(CancellationToken cancellationToken)
    {
        var settings = await GetAsync(cancellationToken);
        return settings.EnrichServiceUrl;
    }

    private static AppSettingsPayload Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new AppSettingsPayload(null, DefaultEnrichServiceUrl, DefaultEnrichChatHistoryDepth);
        }

        try
        {
            var payload = JsonSerializer.Deserialize<AppSettingsPayload>(json, JsonOptions);
            return payload is null
                ? new AppSettingsPayload(null, DefaultEnrichServiceUrl, DefaultEnrichChatHistoryDepth)
                : new AppSettingsPayload(
                    NormalizeUrl(payload.LlmGatewayUrl),
                    NormalizeRequiredUrl(payload.EnrichServiceUrl, DefaultEnrichServiceUrl),
                    NormalizeEnrichChatHistoryDepth(payload.EnrichChatHistoryDepth));
        }
        catch (JsonException)
        {
            return new AppSettingsPayload(null, DefaultEnrichServiceUrl, DefaultEnrichChatHistoryDepth);
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

    private static string NormalizeRequiredUrl(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return value.Trim();
    }

    private static int NormalizeEnrichChatHistoryDepth(int? value)
    {
        var raw = value ?? DefaultEnrichChatHistoryDepth;
        return Math.Clamp(raw, 1, 50);
    }

    private sealed record AppSettingsPayload(string? LlmGatewayUrl, string? EnrichServiceUrl, int? EnrichChatHistoryDepth);
}
