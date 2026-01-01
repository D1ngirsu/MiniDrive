namespace MiniDrive.Common;

/// <summary>
/// Non-generic operation result.
/// </summary>
public class Result
{
    protected Result(bool succeeded, string? error = null)
    {
        Succeeded = succeeded;
        Error = error;
    }

    public bool Succeeded { get; }
    public string? Error { get; }

    public static Result Success() => new(true);

    public static Result Failure(string error) =>
        new(false, string.IsNullOrWhiteSpace(error) ? "Unknown error." : error.Trim());
}

/// <summary>
/// Generic operation result that carries a payload when successful.
/// </summary>
public class Result<T> : Result
{
    protected Result(bool succeeded, T? value, string? error)
        : base(succeeded, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(true, value, null);

    public static new Result<T> Failure(string error) =>
        new(false, default, string.IsNullOrWhiteSpace(error) ? "Unknown error." : error.Trim());
}
