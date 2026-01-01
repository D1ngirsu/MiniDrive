using MiniDrive.Audit.Entities;

namespace MiniDrive.Audit.Services;

/// <summary>
/// Service interface for audit logging operations.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit entry.
    /// </summary>
    Task LogAsync(AuditLog entry);

    /// <summary>
    /// Logs a user action with entity information.
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
        string? userAgent = null);
}
