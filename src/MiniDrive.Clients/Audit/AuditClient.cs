using System.Net.Http.Json;

namespace MiniDrive.Clients.Audit;

/// <summary>
/// HTTP client for Audit service.
/// </summary>
public class AuditClient : IAuditClient
{
    private readonly HttpClient _httpClient;

    public AuditClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task LogActionAsync(
        Guid userId,
        string action,
        string entityType,
        string entityId,
        bool isSuccess = true,
        string? details = null,
        string? errorMessage = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new
            {
                userId,
                action,
                entityType,
                entityId,
                isSuccess,
                details,
                errorMessage,
                ipAddress,
                userAgent
            };

            await _httpClient.PostAsJsonAsync("/api/Audit/log", request, cancellationToken);
            // Fire and forget - don't throw on failure
        }
        catch
        {
            // Silently fail - audit logging should not break the main flow
        }
    }
}

