using Microsoft.EntityFrameworkCore;
using MiniDrive.Quota.Entities;

namespace MiniDrive.Quota.Repositories;

/// <summary>
/// Repository for user quota data access.
/// </summary>
public class QuotaRepository
{
    private readonly QuotaDbContext _context;

    public QuotaRepository(QuotaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets quota for a user, creating a default one if it doesn't exist.
    /// </summary>
    public async Task<UserQuota> GetOrCreateAsync(Guid userId, long defaultLimitBytes = 5L * 1024 * 1024 * 1024) // Default 5GB
    {
        var quota = await _context.UserQuotas
            .FirstOrDefaultAsync(q => q.UserId == userId);

        if (quota != null)
        {
            return quota;
        }

        // Create default quota
        var newQuota = new UserQuota
        {
            UserId = userId,
            UsedBytes = 0,
            LimitBytes = defaultLimitBytes
        };

        _context.UserQuotas.Add(newQuota);
        await _context.SaveChangesAsync();
        return newQuota;
    }

    /// <summary>
    /// Gets quota for a user.
    /// </summary>
    public async Task<UserQuota?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserQuotas
            .FirstOrDefaultAsync(q => q.UserId == userId);
    }

    /// <summary>
    /// Updates the used bytes for a user.
    /// </summary>
    public async Task<bool> UpdateUsedBytesAsync(Guid userId, long usedBytes)
    {
        var quota = await _context.UserQuotas
            .FirstOrDefaultAsync(q => q.UserId == userId);

        if (quota == null)
        {
            return false;
        }

        quota.UsedBytes = usedBytes;
        quota.Touch();
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Increases the used bytes for a user.
    /// </summary>
    public async Task<bool> IncreaseUsedBytesAsync(Guid userId, long bytes)
    {
        var quota = await _context.UserQuotas
            .FirstOrDefaultAsync(q => q.UserId == userId);

        if (quota == null)
        {
            return false;
        }

        quota.UsedBytes += bytes;
        quota.Touch();
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Decreases the used bytes for a user.
    /// </summary>
    public async Task<bool> DecreaseUsedBytesAsync(Guid userId, long bytes)
    {
        var quota = await _context.UserQuotas
            .FirstOrDefaultAsync(q => q.UserId == userId);

        if (quota == null)
        {
            return false;
        }

        quota.UsedBytes = Math.Max(0, quota.UsedBytes - bytes);
        quota.Touch();
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Updates the limit for a user.
    /// </summary>
    public async Task<bool> UpdateLimitAsync(Guid userId, long limitBytes)
    {
        var quota = await _context.UserQuotas
            .FirstOrDefaultAsync(q => q.UserId == userId);

        if (quota == null)
        {
            return false;
        }

        quota.LimitBytes = limitBytes;
        quota.Touch();
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Creates or updates a quota for a user.
    /// </summary>
    public async Task<UserQuota> CreateOrUpdateAsync(UserQuota quota)
    {
        var existing = await _context.UserQuotas
            .FirstOrDefaultAsync(q => q.UserId == quota.UserId);

        if (existing != null)
        {
            quota.Touch();
            _context.Entry(existing).CurrentValues.SetValues(quota);
        }
        else
        {
            _context.UserQuotas.Add(quota);
        }

        await _context.SaveChangesAsync();
        return quota;
    }
}

