using System.Net.Http.Json;
using System.Text.Json;

namespace MiniDrive.Clients.Quota;

/// <summary>
/// HTTP client for Quota service.
/// </summary>
public class QuotaClient : IQuotaClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public QuotaClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<bool> CanUploadAsync(Guid userId, long fileSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/Quota/{userId}/can-upload?fileSize={fileSize}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<CanUploadResponse>(_jsonOptions, cancellationToken);
            return result?.CanUpload ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IncreaseAsync(Guid userId, long bytes, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { userId, bytes };
            var response = await _httpClient.PostAsJsonAsync($"/api/Quota/{userId}/increase", request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DecreaseAsync(Guid userId, long bytes, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new { userId, bytes };
            var response = await _httpClient.PostAsJsonAsync($"/api/Quota/{userId}/decrease", request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserQuotaInfo?> GetQuotaAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/Quota/{userId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserQuotaInfo>(_jsonOptions, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private class CanUploadResponse
    {
        public bool CanUpload { get; set; }
    }
}

