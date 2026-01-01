using MiniDrive.Common;

namespace MiniDrive.Files.Entities;

/// <summary>
/// Represents a file entry in the system.
/// </summary>
public class FileEntry : BaseEntity
{
    /// <summary>
    /// Original file name as uploaded by the user.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// MIME type of the file.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Storage path where the file is physically stored.
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who owns this file.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// ID of the folder containing this file. Null if in root.
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// File extension (e.g., ".pdf", ".jpg").
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Optional description or notes about the file.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the file has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the file was deleted (if soft-deleted).
    /// </summary>
    public DateTime? DeletedAtUtc { get; set; }
}
