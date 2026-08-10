namespace StudentCourseRegistration.Api.Api.Contracts.Common;

/// <summary>A page of items together with paging metadata.</summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class PagedResponse<T>
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

    /// <summary>True when a page follows the current one.</summary>
    public bool HasNext { get; init; }

    /// <summary>True when a page precedes the current one.</summary>
    public bool HasPrevious { get; init; }
}

/// <summary>Builds paged HTTP responses from application paged results.</summary>
public static class PagedResponse
{
    /// <summary>Maps an application paged result to an HTTP paged response.</summary>
    /// <typeparam name="TSource">The source item type.</typeparam>
    /// <typeparam name="TDestination">The destination item type.</typeparam>
    /// <param name="result">The source paged result.</param>
    /// <param name="map">The item mapping function.</param>
    /// <returns>The HTTP paged response.</returns>
    public static PagedResponse<TDestination> From<TSource, TDestination>(
        Application.Common.Pagination.PagedResult<TSource> result,
        Func<TSource, TDestination> map) => new()
        {
            Items = result.Items.Select(map).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages,
            HasNext = result.HasNext,
            HasPrevious = result.HasPrevious
        };

    /// <summary>Maps an already-typed paged result directly, preserving each item.</summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="result">The source paged result.</param>
    /// <returns>The HTTP paged response.</returns>
    public static PagedResponse<T> From<T>(Application.Common.Pagination.PagedResult<T> result) => new()
    {
        Items = result.Items,
        Page = result.Page,
        PageSize = result.PageSize,
        TotalItems = result.TotalItems,
        TotalPages = result.TotalPages,
        HasNext = result.HasNext,
        HasPrevious = result.HasPrevious
    };
}
