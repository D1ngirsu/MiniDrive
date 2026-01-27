namespace MiniDrive.Sharing.DTOs;

/// <summary>
/// Request DTO for updating a share.
/// </summary>
public class UpdateShareRequest
{
    /// <summary>
    /// Permission level: "view", "edit", "admin".
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// New expiration date for the share (UTC). Pass null to remove expiration.
    /// </summary>
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>
    /// Whether the share is active.
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// New password for public shares. Pass empty string to remove password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Maximum number of downloads. Null means unlimited.
    /// </summary>
    public int? MaxDownloads { get; set; }

    /// <summary>
    /// Updated notes about the share.
    /// </summary>
    public string? Notes { get; set; }
}
