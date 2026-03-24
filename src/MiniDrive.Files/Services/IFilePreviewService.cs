using MiniDrive.Common;
using MiniDrive.Files.DTOs;
using MiniDrive.Files.Entities;

namespace MiniDrive.Files.Services;

/// <summary>
/// Interface for file preview operations.
/// </summary>
public interface IFilePreviewService
{
    /// <summary>
    /// Gets a preview of a file.
    /// </summary>
    /// <param name="file">The file entry to preview.</param>
    /// <param name="maxPreviewSize">Maximum preview size in bytes (default: 100KB).</param>
    /// <param name="includeContent">Whether to include actual preview content or just metadata.</param>
    /// <returns>File preview response.</returns>
    Task<FilePreviewResponse> GetPreviewAsync(
        FileEntry file,
        int maxPreviewSize = 100 * 1024,
        bool includeContent = true);

    /// <summary>
    /// Determines if a file type supports preview.
    /// </summary>
    bool SupportsPreview(string contentType, string extension);

    /// <summary>
    /// Gets a thumbnail of an image file.
    /// </summary>
    /// <param name="fileStream">The file stream of the image.</param>
    /// <param name="contentType">The content type of the image.</param>
    /// <param name="maxWidth">Maximum width for thumbnail.</param>
    /// <param name="maxHeight">Maximum height for thumbnail.</param>
    /// <returns>Thumbnail as base64 data URL.</returns>
    Task<string> GetImageThumbnailAsync(
        Stream fileStream,
        string contentType,
        int maxWidth = 200,
        int maxHeight = 200);
}
