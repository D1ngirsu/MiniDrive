namespace MiniDrive.Common;

/// <summary>
/// Base type for entities persisted across the solution.
/// Provides a consistent identifier and basic audit timestamps.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Stable unique identifier for the entity.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// When the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// When the entity was last updated (UTC).
    /// </summary>
    public DateTime? UpdatedAtUtc { get; protected set; }

    /// <summary>
    /// Mark the entity as updated, refreshing the timestamp.
    /// </summary>
    public void Touch(DateTime? updatedAtUtc = null)
    {
        UpdatedAtUtc = updatedAtUtc ?? DateTime.UtcNow;
    }
}
