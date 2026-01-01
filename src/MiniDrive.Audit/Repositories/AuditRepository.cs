using Microsoft.EntityFrameworkCore;
using MiniDrive.Audit.Entities;

namespace MiniDrive.Audit.Repositories;

/// <summary>
/// Repository for audit log data access.
/// </summary>
public class AuditRepository
{
    private readonly AuditDbContext _context;

    public AuditRepository(AuditDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    public async Task<AuditLog> CreateAsync(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
        return auditLog;
    }

    /// <summary>
    /// Gets audit logs by user ID.
    /// </summary>
    public async Task<IReadOnlyCollection<AuditLog>> GetByUserIdAsync(
        Guid userId,
        int? limit = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.AuditLogs
            .Where(log => log.UserId == userId);

        if (fromDate.HasValue)
        {
            query = query.Where(log => log.CreatedAtUtc >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(log => log.CreatedAtUtc <= toDate.Value);
        }

        query = query.OrderByDescending(log => log.CreatedAtUtc);

        if (limit.HasValue && limit.Value > 0)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets audit logs by entity type and ID.
    /// </summary>
    public async Task<IReadOnlyCollection<AuditLog>> GetByEntityAsync(
        string entityType,
        string entityId,
        int? limit = null)
    {
        var query = _context.AuditLogs
            .Where(log => log.EntityType == entityType && log.EntityId == entityId)
            .OrderByDescending(log => log.CreatedAtUtc)
            .AsQueryable();

        if (limit.HasValue && limit.Value > 0)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets audit logs by action type.
    /// </summary>
    public async Task<IReadOnlyCollection<AuditLog>> GetByActionAsync(
        string action,
        int? limit = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.AuditLogs
            .Where(log => log.Action == action);

        if (fromDate.HasValue)
        {
            query = query.Where(log => log.CreatedAtUtc >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(log => log.CreatedAtUtc <= toDate.Value);
        }

        query = query.OrderByDescending(log => log.CreatedAtUtc);

        if (limit.HasValue && limit.Value > 0)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Gets all audit logs with optional filtering.
    /// </summary>
    public async Task<IReadOnlyCollection<AuditLog>> GetAllAsync(
        int? limit = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(log => log.CreatedAtUtc >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(log => log.CreatedAtUtc <= toDate.Value);
        }

        query = query.OrderByDescending(log => log.CreatedAtUtc);

        if (limit.HasValue && limit.Value > 0)
        {
            query = query.Take(limit.Value);
        }

        return await query.ToListAsync();
    }
}

