using Microsoft.Extensions.Options;

namespace MiniDrive.Storage;

/// <summary>
/// Local file system implementation of IFileStorage.
/// Stores files on the local file system in a configurable base directory.
/// </summary>
public class LocalFileStorage : IFileStorage
{
    private readonly StorageOptions _options;
    private readonly string _basePath;

    public LocalFileStorage(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        _basePath = Path.GetFullPath(_options.BasePath);
        
        // Ensure base directory exists
        Directory.CreateDirectory(_basePath);
    }

    /// <summary>
    /// Saves a file stream to storage and returns the relative path.
    /// </summary>
    public async Task<string> SaveAsync(Stream fileStream, string fileName)
    {
        if (fileStream == null || fileStream.Length == 0)
        {
            throw new ArgumentException("File stream cannot be null or empty.", nameof(fileStream));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        }

        // Validate file size
        if (fileStream.Length > _options.MaxFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"File size {fileStream.Length} bytes exceeds maximum allowed size {_options.MaxFileSizeBytes} bytes.");
        }

        // Validate file extension if restrictions are configured
        var extension = Path.GetExtension(fileName);
        if (_options.AllowedExtensions.Count > 0 && !_options.AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"File extension '{extension}' is not allowed. Allowed extensions: {string.Join(", ", _options.AllowedExtensions)}");
        }

        // Generate a unique file name to avoid conflicts
        var sanitizedFileName = SanitizeFileName(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}";
        
        // Create subdirectory structure based on date (YYYY/MM) for better organization
        var now = DateTime.UtcNow;
        var subdirectory = Path.Combine(now.Year.ToString(), now.Month.ToString("D2"));
        var fullDirectory = Path.Combine(_basePath, subdirectory);
        Directory.CreateDirectory(fullDirectory);

        var filePath = Path.Combine(fullDirectory, uniqueFileName);
        var relativePath = Path.Combine(subdirectory, uniqueFileName).Replace('\\', '/');

        // Save the file
        using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            fileStream.Position = 0;
            await fileStream.CopyToAsync(file);
        }

        return relativePath;
    }

    /// <summary>
    /// Retrieves a file stream from storage by path.
    /// </summary>
    public Task<Stream> GetAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        // Security: Ensure the path is within the base directory
        var fullPath = Path.GetFullPath(Path.Combine(_basePath, path));
        if (!fullPath.StartsWith(_basePath, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Access to the specified path is not allowed.");
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found at path: {path}");
        }

        var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream>(fileStream);
    }

    /// <summary>
    /// Deletes a file from storage by path.
    /// </summary>
    public Task DeleteAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        // Security: Ensure the path is within the base directory
        var fullPath = Path.GetFullPath(Path.Combine(_basePath, path));
        if (!fullPath.StartsWith(_basePath, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Access to the specified path is not allowed.");
        }

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the full physical path for a relative storage path.
    /// </summary>
    public string GetFullPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(relativePath));
        }

        var fullPath = Path.GetFullPath(Path.Combine(_basePath, relativePath));
        if (!fullPath.StartsWith(_basePath, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Access to the specified path is not allowed.");
        }

        return fullPath;
    }

    /// <summary>
    /// Sanitizes a file name to remove potentially dangerous characters.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Trim();
    }
}
