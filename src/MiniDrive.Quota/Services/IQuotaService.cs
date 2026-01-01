using MiniDrive.Quota.Entities;

namespace MiniDrive.Quota.Services;

/// <summary>
/// Service interface for quota management operations.
/// </summary>
public interface IQuotaService
{
    /// <summary>
    /// Checks if a user can upload a file of the specified size.
    /// </summary>
    Task<bool> CanUploadAsync(Guid userId, long fileSize);

    /// <summary>
    /// Increases the used storage for a user.
    /// </summary>
    Task<bool> IncreaseAsync(Guid userId, long bytes);

    /// <summary>
    /// Decreases the used storage for a user.
    /// </summary>
    Task<bool> DecreaseAsync(Guid userId, long bytes);

    /// <summary>
    /// Gets the quota information for a user.
    /// </summary>
    Task<UserQuota?> GetQuotaAsync(Guid userId);

    /// <summary>
    /// Gets or creates quota for a user with default limit.
    /// </summary>
    Task<UserQuota> GetOrCreateQuotaAsync(Guid userId, long defaultLimitBytes = 5L * 1024 * 1024 * 1024);

    /// <summary>
    /// Updates the storage limit for a user.
    /// </summary>
    Task<bool> UpdateLimitAsync(Guid userId, long limitBytes);

    /// <summary>
    /// Syncs the used bytes with actual file storage for a user.
    /// </summary>
    Task<bool> SyncUsedBytesAsync(Guid userId, long actualUsedBytes);
}

