namespace MiniDrive.Sharing.DTOs;

/// <summary>
/// Response DTO for share operations.
/// </summary>
public class ShareResponse
{
    /// <summary>
    /// Share ID.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the shared resource.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Type of resource: "file" or "folder".
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the owner who created the share.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// ID of the user the resource is shared with.
    /// </summary>
    public Guid? SharedWithUserId { get; set; }

    /// <summary>
    /// Permission level.
    /// </summary>
    public string Permission { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a public share.
    /// </summary>
    public bool IsPublicShare { get; set; }

    /// <summary>
    /// Public share token (for link shares).
    /// </summary>
    public string? ShareToken { get; set; }

    /// <summary>
    /// Whether the share is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Expiration date (UTC).
    /// </summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>
    /// Whether the share has password protection.
    /// </summary>
    public bool HasPassword { get; set; }

    /// <summary>
    /// Maximum downloads allowed.
    /// </summary>
    public int? MaxDownloads { get; set; }

    /// <summary>
    /// Current download count.
    /// </summary>
    public int CurrentDownloads { get; set; }

    /// <summary>
    /// Notes about the share.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// When the share was created (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// When the share was last updated (UTC).
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }
}
