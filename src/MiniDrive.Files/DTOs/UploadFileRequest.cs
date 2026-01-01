namespace MiniDrive.Files.DTOs;

/// <summary>
/// Request DTO for uploading a file.
/// </summary>
public class UploadFileRequest
{
    /// <summary>
    /// Optional folder ID where the file should be uploaded.
    /// </summary>
    public Guid? FolderId { get; set; }

    /// <summary>
    /// Optional description for the file.
    /// </summary>
    public string? Description { get; set; }
}
