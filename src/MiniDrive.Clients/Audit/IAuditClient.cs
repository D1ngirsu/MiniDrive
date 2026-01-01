namespace MiniDrive.Clients.Audit;

/// <summary>
/// Client interface for Audit service operations.
/// </summary>
public interface IAuditClient
{
    /// <summary>
    /// Logs an audit action.
    /// </summary>
    Task LogActionAsync(
        Guid userId,
        string action,
        string entityType,
        string entityId,
        bool isSuccess = true,
        string? details = null,
        string? errorMessage = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
}

