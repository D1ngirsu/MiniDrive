namespace MiniDrive.Storage;

/// <summary>
/// Configuration options for file storage.
/// </summary>
public class StorageOptions
{
    public const string ConfigurationSectionName = "Storage";

    /// <summary>
    /// Base directory path where files will be stored.
    /// </summary>
    public string BasePath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "storage");

    /// <summary>
    /// Maximum file size in bytes. Default is 100MB.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 100 * 1024 * 1024;

    /// <summary>
    /// Allowed file extensions. If empty, all extensions are allowed.
    /// </summary>
    public HashSet<string> AllowedExtensions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
