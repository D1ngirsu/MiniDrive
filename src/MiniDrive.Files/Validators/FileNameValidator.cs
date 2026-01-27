using MiniDrive.Common;

namespace MiniDrive.Files.Validators;

/// <summary>
/// Validator for file names and search terms to prevent security vulnerabilities.
/// </summary>
public static class FileNameValidator
{
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars()
        .Concat(new[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|' })
        .ToArray();

    /// <summary>
    /// Validates a file name for security and format issues.
    /// </summary>
    public static Result ValidateFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Result.Failure("File name cannot be empty.");

        if (fileName.Length > 255)
            return Result.Failure("File name exceeds maximum length of 255 characters.");

        if (fileName.Any(c => InvalidChars.Contains(c)))
            return Result.Failure("File name contains invalid characters.");

        if (fileName.Contains("..") || fileName.StartsWith('.'))
            return Result.Failure("File name cannot contain path traversal patterns.");

        if (fileName.Contains("\0"))
            return Result.Failure("File name contains null bytes.");

        return Result.Success();
    }

    /// <summary>
    /// Validates a search term to prevent injection attacks.
    /// </summary>
    public static Result ValidateSearchTerm(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Result.Success(); // Empty search is valid

        if (searchTerm.Length > 1000)
            return Result.Failure("Search term exceeds maximum length of 1000 characters.");

        if (searchTerm.Contains("\0"))
            return Result.Failure("Search term contains null bytes.");

        return Result.Success();
    }

    /// <summary>
    /// Validates a description to prevent injection attacks.
    /// </summary>
    public static Result ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Success(); // Empty description is valid

        if (description.Length > 5000)
            return Result.Failure("Description exceeds maximum length of 5000 characters.");

        if (description.Contains("\0"))
            return Result.Failure("Description contains null bytes.");

        return Result.Success();
    }
}
