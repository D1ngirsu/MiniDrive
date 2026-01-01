namespace MiniDrive.Folders.DTOs;

/// <summary>
/// Request DTO for updating a folder.
/// </summary>
public class UpdateFolderRequest
{
    /// <summary>
    /// New name for the folder.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// New description for the folder.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// New color identifier for the folder.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// New parent folder ID (for moving folders).
    /// </summary>
    public Guid? ParentFolderId { get; set; }
}

