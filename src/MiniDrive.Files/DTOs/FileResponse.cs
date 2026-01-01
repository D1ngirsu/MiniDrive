namespace MiniDrive.Files.DTOs;

/// <summary>
/// Response DTO for file information.
/// </summary>
public class FileResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Extension { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public Guid? FolderId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Human-readable file size (e.g., "1.5 MB").
    /// </summary>
    public string FormattedSize { get; set; } = string.Empty;
}
