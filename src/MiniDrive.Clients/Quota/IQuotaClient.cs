namespace MiniDrive.Clients.Quota;

/// <summary>
/// Client interface for Quota service operations.
/// </summary>
public interface IQuotaClient
{
    /// <summary>
    /// Checks if a user can upload a file of the specified size.
    /// </summary>
    Task<bool> CanUploadAsync(Guid userId, long fileSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Increases the used storage for a user.
    /// </summary>
    Task<bool> IncreaseAsync(Guid userId, long bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decreases the used storage for a user.
    /// </summary>
    Task<bool> DecreaseAsync(Guid userId, long bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the quota information for a user.
    /// </summary>
    Task<UserQuotaInfo?> GetQuotaAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// User quota information.
/// </summary>
public class UserQuotaInfo
{
    public Guid UserId { get; set; }
    public long UsedBytes { get; set; }
    public long LimitBytes { get; set; }
    public long AvailableBytes => LimitBytes - UsedBytes;
}

