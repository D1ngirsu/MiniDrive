namespace MiniDrive.Common.Exceptions;

public class MiniDriveException : Exception
{
    public MiniDriveException(string message)
        : base(message)
    {
    }

    public MiniDriveException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}

public sealed class NotFoundException : MiniDriveException
{
    public NotFoundException(string resourceName, string? resourceId = null)
        : base(resourceId is null
            ? $"{resourceName} was not found."
            : $"{resourceName} with id '{resourceId}' was not found.")
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }

    public string ResourceName { get; }
    public string? ResourceId { get; }
}

public sealed class ValidationException : MiniDriveException
{
    public ValidationException(string message, IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Errors = errors ?? new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

