namespace MiniDrive.Files.DTOs;

/// <summary>
/// Response DTO for file preview information.
/// </summary>
public class FilePreviewResponse
{
    /// <summary>
    /// File ID.
    /// </summary>
    public Guid FileId { get; set; }

    /// <summary>
    /// Original file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File MIME type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File extension.
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Type of preview available.
    /// </summary>
    public PreviewType PreviewType { get; set; }

    /// <summary>
    /// Whether preview is available for this file.
    /// </summary>
    public bool IsPreviewAvailable { get; set; }

    /// <summary>
    /// Preview content (varies by type: base64 for images, text for text files, etc.).
    /// </summary>
    public string? PreviewContent { get; set; }

    /// <summary>
    /// Preview data URL (for images, etc.). Can be used directly in img src.
    /// </summary>
    public string? PreviewDataUrl { get; set; }

    /// <summary>
    /// Number of lines/characters in preview (for text files).
    /// </summary>
    public int PreviewSize { get; set; }

    /// <summary>
    /// Message explaining why preview is not available.
    /// </summary>
    public string? PreviewUnavailableReason { get; set; }
}

/// <summary>
/// Enum for different types of file previews.
/// </summary>
public enum PreviewType
{
    /// <summary>No preview available.</summary>
    None = 0,

    /// <summary>Text-based preview (txt, json, xml, csv, etc.).</summary>
    Text = 1,

    /// <summary>Image preview (jpg, png, gif, webp, etc.).</summary>
    Image = 2,

    /// <summary>PDF preview information.</summary>
    Pdf = 3,

    /// <summary>Document preview (docx, xlsx, pptx, etc.).</summary>
    Document = 4,

    /// <summary>Audio file preview (metadata only).</summary>
    Audio = 5,

    /// <summary>Video file preview (metadata only).</summary>
    Video = 6,

    /// <summary>Code file preview (source code).</summary>
    Code = 7,

    /// <summary>Archive preview (zip, rar, tar, etc.).</summary>
    Archive = 8
}
