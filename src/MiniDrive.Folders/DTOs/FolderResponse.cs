namespace MiniDrive.Folders.DTOs;

/// <summary>
/// Response DTO for folder information.
/// </summary>
public class FolderResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public Guid? ParentFolderId { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

