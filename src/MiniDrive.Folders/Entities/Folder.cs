using MiniDrive.Common;

namespace MiniDrive.Folders.Entities;

/// <summary>
/// Represents a folder in the file system.
/// </summary>
public class Folder : BaseEntity
{
    /// <summary>
    /// Name of the folder.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who owns this folder.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// ID of the parent folder. Null if this is a root folder.
    /// </summary>
    public Guid? ParentFolderId { get; set; }

    /// <summary>
    /// Optional description of the folder.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Color or icon identifier for UI customization.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Whether the folder has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the folder was deleted (if soft-deleted).
    /// </summary>
    public DateTime? DeletedAtUtc { get; set; }
}
