using MiniDrive.Common;

namespace MiniDrive.Quota.Entities;

/// <summary>
/// Represents storage quota information for a user.
/// </summary>
public class UserQuota : BaseEntity
{
    /// <summary>
    /// ID of the user this quota belongs to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Total storage used by the user in bytes.
    /// </summary>
    public long UsedBytes { get; set; }

    /// <summary>
    /// Maximum storage limit for the user in bytes.
    /// </summary>
    public long LimitBytes { get; set; }

    /// <summary>
    /// Gets the available storage space in bytes.
    /// </summary>
    public long AvailableBytes => Math.Max(0, LimitBytes - UsedBytes);

    /// <summary>
    /// Gets the percentage of storage used.
    /// </summary>
    public double UsagePercentage => LimitBytes > 0 ? (double)UsedBytes / LimitBytes * 100 : 0;

    /// <summary>
    /// Checks if the user has exceeded their quota.
    /// </summary>
    public bool IsExceeded => UsedBytes > LimitBytes;

    /// <summary>
    /// Checks if the user can store additional bytes.
    /// </summary>
    public bool CanStore(long bytes) => UsedBytes + bytes <= LimitBytes;
}
