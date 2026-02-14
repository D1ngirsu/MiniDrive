using MiniDrive.Common;

namespace MiniDrive.Sharing.Entities;

/// <summary>
/// Represents a shared file or folder with specific permissions.
/// </summary>
public class Share : BaseEntity
{
    /// <summary>
    /// ID of the resource (file or folder) being shared.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Type of resource: "file" or "folder".
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who owns/created the share.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// ID of the user who the resource is shared with. Null for public/link shares.
    /// </summary>
    public Guid? SharedWithUserId { get; set; }

    /// <summary>
    /// Permission level: "view", "edit", "admin".
    /// </summary>
    public string Permission { get; set; } = "view";

    /// <summary>
    /// Whether this share is a public link share (no specific user).
    /// </summary>
    public bool IsPublicShare { get; set; }

    /// <summary>
    /// Public share token/slug for link-based sharing.
    /// </summary>
    public string? ShareToken { get; set; }

    /// <summary>
    /// Whether the share is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional expiration date for the share. Null means no expiration.
    /// </summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>
    /// Whether the share has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Optional password protection for public shares.
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Maximum number of downloads allowed (for public links). Null means unlimited.
    /// </summary>
    public int? MaxDownloads { get; set; }

    /// <summary>
    /// Current number of downloads (for public links).
    /// </summary>
    public int CurrentDownloads { get; set; }

    /// <summary>
    /// Optional notes about why this resource is shared.
    /// </summary>
    public string? Notes { get; set; }
}
