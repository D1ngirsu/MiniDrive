using MiniDrive.Audit.Entities;
using MiniDrive.Audit.Repositories;

namespace MiniDrive.Audit.Services;

/// <summary>
/// Service for audit logging operations.
/// </summary>
public class AuditService : IAuditService
{
    private readonly AuditRepository _auditRepository;

    public AuditService(AuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    /// <summary>
    /// Logs an audit entry.
    /// </summary>
    public async Task LogAsync(AuditLog entry)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        await _auditRepository.CreateAsync(entry);
    }

    /// <summary>
    /// Logs a user action with entity information.
    /// </summary>
    public async Task LogActionAsync(
        Guid userId,
        string action,
        string entityType,
        string entityId,
        bool isSuccess = true,
        string? details = null,
        string? errorMessage = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Action cannot be null or empty.", nameof(action));
        }

        if (string.IsNullOrWhiteSpace(entityType))
        {
            throw new ArgumentException("EntityType cannot be null or empty.", nameof(entityType));
        }

        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new ArgumentException("EntityId cannot be null or empty.", nameof(entityId));
        }

        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            IsSuccess = isSuccess,
            Details = details,
            ErrorMessage = errorMessage,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await _auditRepository.CreateAsync(auditLog);
    }
}

