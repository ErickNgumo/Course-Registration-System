namespace StudentCourseRegistration.Api.Application.Common.Pagination;

/// <summary>A page of results together with paging metadata.</summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>The items on the current page.</summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>The current 1-based page number.</summary>
    public int Page { get; init; }

    /// <summary>The maximum number of items per page.</summary>
    public int PageSize { get; init; }

    /// <summary>The total number of items across all pages.</summary>
    public int TotalItems { get; init; }

    /// <summary>The total number of pages.</summary>
    public int TotalPages { get; init; }

    /// <summary>True when there is a page after the current one.</summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>True when there is a page before the current one.</summary>
    public bool HasPrevious => Page > 1;
}

/// <summary>Reusable paging parameters.</summary>
public sealed class PageQuery
{
    private int _page = 1;
    private int _pageSize = 25;

    /// <summary>The 1-based page number, clamped to at least 1.</summary>
    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    /// <summary>The maximum number of items per page, clamped between 1 and 100.</summary>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value switch
        {
            < 1 => 1,
            > 100 => 100,
            _ => value
        };
    }

    /// <summary>The number of items to skip.</summary>
    public int Skip => (Page - 1) * PageSize;
}
