using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MedAssist.Application.Services;
using MedAssist.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MedAssist.Infrastructure.Services;

public sealed class EnrichPatientCardsSyncService
{
    private readonly ISystemSettingsService _systemSettingsService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EnrichPatientCardsSyncService> _logger;

    public EnrichPatientCardsSyncService(
        ISystemSettingsService systemSettingsService,
        IConfiguration configuration,
        ILogger<EnrichPatientCardsSyncService> logger)
    {
        _systemSettingsService = systemSettingsService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task TryUpsertAsync(Patient patient, Doctor? doctor, CancellationToken cancellationToken)
    {
        try
        {
            var baseUrl = await _systemSettingsService.GetEnrichServiceUrlAsync(cancellationToken);
            var apiKey = ResolveApiKey();
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Skipping enrich patient upsert because settings are not configured.");
                return;
            }

            var payload = new PatientCardUpsertRequest
            {
                PatientId = patient.Id,
                SpecialtyCode = ResolveSpecialtyCode(doctor),
                History = new PatientCardHistoryRequest
                {
                    Sex = patient.Sex is null ? null : (int)patient.Sex.Value,
                    AgeYears = patient.AgeYears,
                    Allergies = NormalizeNullableString(patient.Allergies),
                    ChronicConditions = NormalizeNullableString(patient.ChronicConditions),
                    Tags = NormalizeNullableString(patient.Tags),
                    Status = (int)patient.Status,
                    Notes = NormalizeNullableString(patient.Notes)
                }
            };

            var endpoint = $"{baseUrl.TrimEnd('/')}/v1/patient-cards";
            using var client = new HttpClient();
            var putResult = await SendUpsertAsync(client, endpoint, apiKey, payload, HttpMethod.Put, cancellationToken);
            if (putResult.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Enrich currently treats PUT as update-only. Fallback to POST for create.
                var postResult = await SendUpsertAsync(client, endpoint, apiKey, payload, HttpMethod.Post, cancellationToken);
                if (!postResult.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Enrich patient upsert fallback POST failed for patientId={PatientId}. Status={Status}. Body={Body}",
                        patient.Id,
                        (int)postResult.StatusCode,
                        postResult.Body);
                }
            }
            else if (!putResult.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Enrich patient upsert failed for patientId={PatientId}. Status={Status}. Body={Body}",
                    patient.Id,
                    (int)putResult.StatusCode,
                    putResult.Body);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("Enrich patient upsert canceled for patientId={PatientId}", patient.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Enrich patient upsert threw for patientId={PatientId}", patient.Id);
        }
    }

    public async Task TryDeleteAsync(Guid patientId, CancellationToken cancellationToken)
    {
        try
        {
            var baseUrl = await _systemSettingsService.GetEnrichServiceUrlAsync(cancellationToken);
            var apiKey = ResolveApiKey();
            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Skipping enrich patient delete because settings are not configured.");
                return;
            }

            var endpoint = $"{baseUrl.TrimEnd('/')}/v1/patient-cards/{patientId:D}";
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            request.Headers.Add("X-Api-Key", apiKey);

            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "Enrich patient delete failed for patientId={PatientId}. Status={Status}. Body={Body}",
                    patientId,
                    (int)response.StatusCode,
                    body);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("Enrich patient delete canceled for patientId={PatientId}", patientId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Enrich patient delete threw for patientId={PatientId}", patientId);
        }
    }

    private string? ResolveApiKey()
    {
        var apiKey = _configuration["Enrich:ApiKey"] ?? Environment.GetEnvironmentVariable("ENRICH_API_KEY");
        return string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim();
    }

    private static string? ResolveSpecialtyCode(Doctor? doctor)
    {
        var code = doctor?.SpecializationCodes?.FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value));
        return string.IsNullOrWhiteSpace(code) ? null : code.Trim();
    }

    private static string? NormalizeNullableString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static async Task<EnrichResponseResult> SendUpsertAsync(
        HttpClient client,
        string endpoint,
        string apiKey,
        PatientCardUpsertRequest payload,
        HttpMethod method,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, endpoint)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("X-Api-Key", apiKey);

        using var response = await client.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return new EnrichResponseResult(response.IsSuccessStatusCode, response.StatusCode, body);
    }

    private sealed record PatientCardUpsertRequest
    {
        [JsonPropertyName("PatientId")]
        public Guid PatientId { get; init; }

        [JsonPropertyName("SpecialtyCode")]
        public string? SpecialtyCode { get; init; }

        [JsonPropertyName("History")]
        public PatientCardHistoryRequest History { get; init; } = new();
    }

    private sealed record PatientCardHistoryRequest
    {
        [JsonPropertyName("sex")]
        public int? Sex { get; init; }

        [JsonPropertyName("ageYears")]
        public int? AgeYears { get; init; }

        [JsonPropertyName("allergies")]
        public string? Allergies { get; init; }

        [JsonPropertyName("chronicConditions")]
        public string? ChronicConditions { get; init; }

        [JsonPropertyName("tags")]
        public string? Tags { get; init; }

        [JsonPropertyName("status")]
        public int Status { get; init; }

        [JsonPropertyName("notes")]
        public string? Notes { get; init; }
    }

    private sealed record EnrichResponseResult(
        bool IsSuccessStatusCode,
        System.Net.HttpStatusCode StatusCode,
        string Body);
}
