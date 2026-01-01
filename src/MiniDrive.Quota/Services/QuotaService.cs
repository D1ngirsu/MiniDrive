using MiniDrive.Quota.Entities;
using MiniDrive.Quota.Repositories;

namespace MiniDrive.Quota.Services;

/// <summary>
/// Service for quota management operations.
/// </summary>
public class QuotaService : IQuotaService
{
    private readonly QuotaRepository _quotaRepository;

    public QuotaService(QuotaRepository quotaRepository)
    {
        _quotaRepository = quotaRepository;
    }

    /// <summary>
    /// Checks if a user can upload a file of the specified size.
    /// </summary>
    public async Task<bool> CanUploadAsync(Guid userId, long fileSize)
    {
        if (fileSize < 0)
        {
            return false;
        }

        var quota = await _quotaRepository.GetOrCreateAsync(userId);
        return quota.CanStore(fileSize);
    }

    /// <summary>
    /// Increases the used storage for a user.
    /// </summary>
    public async Task<bool> IncreaseAsync(Guid userId, long bytes)
    {
        if (bytes < 0)
        {
            throw new ArgumentException("Bytes cannot be negative.", nameof(bytes));
        }

        return await _quotaRepository.IncreaseUsedBytesAsync(userId, bytes);
    }

    /// <summary>
    /// Decreases the used storage for a user.
    /// </summary>
    public async Task<bool> DecreaseAsync(Guid userId, long bytes)
    {
        if (bytes < 0)
        {
            throw new ArgumentException("Bytes cannot be negative.", nameof(bytes));
        }

        return await _quotaRepository.DecreaseUsedBytesAsync(userId, bytes);
    }

    /// <summary>
    /// Gets the quota information for a user.
    /// </summary>
    public async Task<UserQuota?> GetQuotaAsync(Guid userId)
    {
        return await _quotaRepository.GetByUserIdAsync(userId);
    }

    /// <summary>
    /// Gets or creates quota for a user with default limit.
    /// </summary>
    public async Task<UserQuota> GetOrCreateQuotaAsync(Guid userId, long defaultLimitBytes = 5L * 1024 * 1024 * 1024)
    {
        return await _quotaRepository.GetOrCreateAsync(userId, defaultLimitBytes);
    }

    /// <summary>
    /// Updates the storage limit for a user.
    /// </summary>
    public async Task<bool> UpdateLimitAsync(Guid userId, long limitBytes)
    {
        if (limitBytes < 0)
        {
            throw new ArgumentException("Limit bytes cannot be negative.", nameof(limitBytes));
        }

        return await _quotaRepository.UpdateLimitAsync(userId, limitBytes);
    }

    /// <summary>
    /// Syncs the used bytes with actual file storage for a user.
    /// </summary>
    public async Task<bool> SyncUsedBytesAsync(Guid userId, long actualUsedBytes)
    {
        if (actualUsedBytes < 0)
        {
            throw new ArgumentException("Used bytes cannot be negative.", nameof(actualUsedBytes));
        }

        return await _quotaRepository.UpdateUsedBytesAsync(userId, actualUsedBytes);
    }
}

