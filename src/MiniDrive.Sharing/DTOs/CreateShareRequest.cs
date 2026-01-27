namespace MiniDrive.Sharing.DTOs;

/// <summary>
/// Request DTO for creating a share.
/// </summary>
public class CreateShareRequest
{
    /// <summary>
    /// ID of the resource (file or folder) to share.
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Type of resource: "file" or "folder".
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user to share with. Null for public/link shares.
    /// </summary>
    public Guid? SharedWithUserId { get; set; }

    /// <summary>
    /// Permission level: "view" (read-only), "edit" (read-write), "admin" (full control).
    /// Default: "view".
    /// </summary>
    public string Permission { get; set; } = "view";

    /// <summary>
    /// Whether to create a public link share.
    /// </summary>
    public bool IsPublicShare { get; set; }

    /// <summary>
    /// Optional password for public link shares.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Optional expiration date for the share (UTC).
    /// </summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>
    /// Maximum number of downloads for public links. Null means unlimited.
    /// </summary>
    public int? MaxDownloads { get; set; }

    /// <summary>
    /// Optional notes about the share.
    /// </summary>
    public string? Notes { get; set; }
}
