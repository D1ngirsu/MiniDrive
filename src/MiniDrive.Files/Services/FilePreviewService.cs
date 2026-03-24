using System.Text;
using MiniDrive.Files.DTOs;
using MiniDrive.Files.Entities;
using MiniDrive.Storage;

namespace MiniDrive.Files.Services;

/// <summary>
/// Service for generating file previews.
/// </summary>
public class FilePreviewService : IFilePreviewService
{
    private readonly IFileStorage _fileStorage;

    // Supported text file extensions
    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".md", ".json", ".xml", ".csv", ".log", ".html", ".htm", ".css", ".js", ".ts",
        ".jsx", ".tsx", ".java", ".cs", ".cpp", ".c", ".h", ".py", ".rb", ".php", ".sql", ".yml", ".yaml"
    };

    // Supported image extensions
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".bmp", ".ico"
    };

    // Supported document extensions
    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".pptx", ".ppt", ".odt", ".ods", ".odp"
    };

    // Supported video extensions
    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".avi", ".mov", ".mkv", ".wmv", ".flv", ".webm", ".m4v", ".mpg", ".mpeg"
    };

    // Supported audio extensions
    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a", ".aiff"
    };

    // Supported archive extensions
    private static readonly HashSet<string> ArchiveExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".iso"
    };

    // MIME types for different categories
    private static readonly Dictionary<string, PreviewType> MimeTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Text files
        { "text/plain", PreviewType.Text },
        { "text/html", PreviewType.Text },
        { "text/css", PreviewType.Text },
        { "text/javascript", PreviewType.Code },
        { "application/json", PreviewType.Text },
        { "application/xml", PreviewType.Text },
        { "text/xml", PreviewType.Text },
        { "text/csv", PreviewType.Text },
        { "text/markdown", PreviewType.Text },

        // Images
        { "image/jpeg", PreviewType.Image },
        { "image/png", PreviewType.Image },
        { "image/gif", PreviewType.Image },
        { "image/webp", PreviewType.Image },
        { "image/svg+xml", PreviewType.Image },
        { "image/x-icon", PreviewType.Image },
        { "image/bmp", PreviewType.Image },

        // Documents
        { "application/pdf", PreviewType.Pdf },
        { "application/msword", PreviewType.Document },
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", PreviewType.Document },
        { "application/vnd.ms-excel", PreviewType.Document },
        { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", PreviewType.Document },
        { "application/vnd.ms-powerpoint", PreviewType.Document },
        { "application/vnd.openxmlformats-officedocument.presentationml.presentation", PreviewType.Document },

        // Audio
        { "audio/mpeg", PreviewType.Audio },
        { "audio/wav", PreviewType.Audio },
        { "audio/flac", PreviewType.Audio },
        { "audio/aac", PreviewType.Audio },
        { "audio/ogg", PreviewType.Audio },
        { "audio/x-m4a", PreviewType.Audio },

        // Video
        { "video/mp4", PreviewType.Video },
        { "video/mpeg", PreviewType.Video },
        { "video/quicktime", PreviewType.Video },
        { "video/x-msvideo", PreviewType.Video },
        { "video/x-matroska", PreviewType.Video },
        { "video/webm", PreviewType.Video },

        // Archives
        { "application/zip", PreviewType.Archive },
        { "application/x-rar-compressed", PreviewType.Archive },
        { "application/x-7z-compressed", PreviewType.Archive },
        { "application/x-tar", PreviewType.Archive },
        { "application/gzip", PreviewType.Archive }
    };

    public FilePreviewService(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
    }

    /// <summary>
    /// Gets a preview of a file.
    /// </summary>
    public async Task<FilePreviewResponse> GetPreviewAsync(
        FileEntry file,
        int maxPreviewSize = 100 * 1024,
        bool includeContent = true)
    {
        var response = new FilePreviewResponse
        {
            FileId = file.Id,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Extension = file.Extension,
            SizeBytes = file.SizeBytes,
            PreviewType = DeterminePreviewType(file.ContentType, file.Extension)
        };

        // Check if preview is supported
        if (response.PreviewType == PreviewType.None)
        {
            response.IsPreviewAvailable = false;
            response.PreviewUnavailableReason = "Preview not available for this file type.";
            return response;
        }

        // Check if preview content should be included
        if (!includeContent)
        {
            response.IsPreviewAvailable = true;
            return response;
        }

        try
        {
            // Open the file from storage
            var fileStream = await _fileStorage.GetAsync(file.StoragePath);
            using (fileStream)
            {
                // Check if file is too large
                if (file.SizeBytes > maxPreviewSize)
                {
                    response.PreviewSize = maxPreviewSize;
                    response.PreviewUnavailableReason = $"File is too large. Showing first {FormatFileSize(maxPreviewSize)}.";
                }
                else
                {
                    response.PreviewSize = (int)file.SizeBytes;
                }

                // Generate preview based on type
                response = response.PreviewType switch
                {
                    PreviewType.Text => await GetTextPreviewAsync(response, fileStream, maxPreviewSize),
                    PreviewType.Code => await GetCodePreviewAsync(response, fileStream, maxPreviewSize),
                    PreviewType.Image => await GetImagePreviewAsync(response, fileStream),
                    PreviewType.Pdf => GetPdfPreviewAsync(response),
                    PreviewType.Audio => GetAudioPreviewAsync(response),
                    PreviewType.Video => GetVideoPreviewAsync(response),
                    PreviewType.Document => GetDocumentPreviewAsync(response),
                    PreviewType.Archive => GetArchivePreviewAsync(response),
                    _ => response
                };

                response.IsPreviewAvailable = true;
                return response;
            }
        }
        catch (Exception ex)
        {
            response.IsPreviewAvailable = false;
            response.PreviewUnavailableReason = $"Error generating preview: {ex.Message}";
            return response;
        }
    }

    /// <summary>
    /// Determines if a file type supports preview.
    /// </summary>
    public bool SupportsPreview(string contentType, string extension)
    {
        var previewType = DeterminePreviewType(contentType, extension);
        return previewType != PreviewType.None;
    }

    /// <summary>
    /// Gets a thumbnail of an image file.
    /// </summary>
    public async Task<string> GetImageThumbnailAsync(
        Stream fileStream,
        string contentType,
        int maxWidth = 200,
        int maxHeight = 200)
    {
        if (!ImageExtensions.Any(ext => contentType.Contains(ext.TrimStart('.'))))
        {
            throw new InvalidOperationException("File is not a supported image format.");
        }

        // Read image data
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var imageData = memoryStream.ToArray();

        // Create base64 data URL
        var base64 = Convert.ToBase64String(imageData);
        return $"data:{contentType};base64,{base64}";
    }

    private PreviewType DeterminePreviewType(string contentType, string extension)
    {
        // Check MIME type first
        if (MimeTypeMap.TryGetValue(contentType, out var previewType))
        {
            return previewType;
        }

        // Check by extension
        if (TextExtensions.Contains(extension))
            return PreviewType.Text;
        if (ImageExtensions.Contains(extension))
            return PreviewType.Image;
        if (DocumentExtensions.Contains(extension))
            return PreviewType.Document;
        if (VideoExtensions.Contains(extension))
            return PreviewType.Video;
        if (AudioExtensions.Contains(extension))
            return PreviewType.Audio;
        if (ArchiveExtensions.Contains(extension))
            return PreviewType.Archive;

        // Check if it's code based on MIME type
        if (contentType.Contains("javascript") || contentType.Contains("typescript") ||
            contentType.Contains("x-python") || contentType.Contains("x-java"))
            return PreviewType.Code;

        return PreviewType.None;
    }

    private async Task<FilePreviewResponse> GetTextPreviewAsync(
        FilePreviewResponse response,
        Stream fileStream,
        int maxSize)
    {
        using var reader = new StreamReader(fileStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var buffer = new char[maxSize];
        var charsRead = await reader.ReadAsync(buffer, 0, maxSize);

        response.PreviewContent = new string(buffer, 0, charsRead);
        response.PreviewSize = charsRead;
        return response;
    }

    private async Task<FilePreviewResponse> GetCodePreviewAsync(
        FilePreviewResponse response,
        Stream fileStream,
        int maxSize)
    {
        // Code preview is similar to text, but could include syntax info
        return await GetTextPreviewAsync(response, fileStream, maxSize);
    }

    private Task<FilePreviewResponse> GetImagePreviewAsync(
        FilePreviewResponse response,
        Stream fileStream)
    {
        // For images, we'll include the image as base64 data URL if file is small
        return Task.Run(async () =>
        {
            if (response.SizeBytes > 5 * 1024 * 1024) // 5MB limit for full image preview
            {
                response.PreviewUnavailableReason = "Image file is too large to preview inline.";
                return response;
            }

            using var memoryStream = new MemoryStream();
            fileStream.Seek(0, SeekOrigin.Begin);
            await fileStream.CopyToAsync(memoryStream);
            var imageData = memoryStream.ToArray();

            var base64 = Convert.ToBase64String(imageData);
            response.PreviewDataUrl = $"data:{response.ContentType};base64,{base64}";
            response.PreviewContent = response.PreviewDataUrl;
            response.PreviewSize = imageData.Length;
            return response;
        });
    }

    private FilePreviewResponse GetPdfPreviewAsync(FilePreviewResponse response)
    {
        response.PreviewUnavailableReason = "PDF preview requires client-side PDF rendering (use PDF.js).";
        return response;
    }

    private FilePreviewResponse GetAudioPreviewAsync(FilePreviewResponse response)
    {
        response.PreviewContent = $"Audio file - {response.FileName}";
        return response;
    }

    private FilePreviewResponse GetVideoPreviewAsync(FilePreviewResponse response)
    {
        response.PreviewContent = $"Video file - {response.FileName}";
        return response;
    }

    private FilePreviewResponse GetDocumentPreviewAsync(FilePreviewResponse response)
    {
        response.PreviewUnavailableReason = "Document preview requires server-side document conversion.";
        return response;
    }

    private FilePreviewResponse GetArchivePreviewAsync(FilePreviewResponse response)
    {
        response.PreviewUnavailableReason = "Archive content preview not implemented.";
        return response;
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
