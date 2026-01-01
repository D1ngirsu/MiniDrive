using MiniDrive.Audit.Entities;
using MiniDrive.Audit.Services;
using MiniDrive.Clients.Audit;

namespace MiniDrive.Files.Api.Adapters;

/// <summary>
/// Adapter that implements IAuditService using HTTP client calls to Audit microservice.
/// </summary>
public class AuditServiceAdapter : IAuditService
{
    private readonly IAuditClient _auditClient;

    public AuditServiceAdapter(IAuditClient auditClient)
    {
        _auditClient = auditClient;
    }

    public Task LogAsync(AuditLog entry)
    {
        return _auditClient.LogActionAsync(
            entry.UserId,
            entry.Action,
            entry.EntityType,
            entry.EntityId,
            entry.IsSuccess,
            entry.Details,
            entry.ErrorMessage,
            entry.IpAddress,
            entry.UserAgent);
    }

    public Task LogActionAsync(
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
        return _auditClient.LogActionAsync(
            userId,
            action,
            entityType,
            entityId,
            isSuccess,
            details,
            errorMessage,
            ipAddress,
            userAgent);
    }
}

