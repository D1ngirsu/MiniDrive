using MiniDrive.Common;

namespace MiniDrive.Audit.Entities;

/// <summary>
/// Represents an audit log entry for tracking user actions and system events.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// Action performed (e.g., "FileUpload", "FileDelete", "FolderCreate").
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity affected (e.g., "File", "Folder", "User").
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity affected.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who performed the action.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Additional details about the action (JSON or text).
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// IP address of the user (if available).
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string (if available).
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Whether the action was successful.
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Error message if the action failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
