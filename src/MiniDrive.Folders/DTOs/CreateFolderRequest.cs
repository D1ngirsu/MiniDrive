namespace MiniDrive.Folders.DTOs;

/// <summary>
/// Request DTO for creating a folder.
/// </summary>
public class CreateFolderRequest
{
    /// <summary>
    /// Name of the folder.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional parent folder ID. If null, folder is created in root.
    /// </summary>
    public Guid? ParentFolderId { get; set; }

    /// <summary>
    /// Optional description of the folder.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional color identifier for UI customization.
    /// </summary>
    public string? Color { get; set; }
}

