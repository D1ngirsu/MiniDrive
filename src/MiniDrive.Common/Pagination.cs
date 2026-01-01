namespace MiniDrive.Common;

/// <summary>
/// Pagination request parameters with guardrails.
/// </summary>
public sealed class Pagination
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int PageNumber { get; }
    public int PageSize { get; }

    public int Skip => (PageNumber - 1) * PageSize;
    public int Take => PageSize;

    public Pagination(int pageNumber = 1, int pageSize = DefaultPageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 1 : Math.Min(pageSize, MaxPageSize);
    }

    public Pagination NextPage() => new(PageNumber + 1, PageSize);
}

/// <summary>
/// Standard paged result wrapper.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int PageNumber,
    int PageSize,
    long TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
