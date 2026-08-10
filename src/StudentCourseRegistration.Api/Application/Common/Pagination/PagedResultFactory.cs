namespace StudentCourseRegistration.Api.Application.Common.Pagination;

/// <summary>Constructs <see cref="PagedResult{T}"/> instances from raw page slices.</summary>
public static class PagedResultFactory
{
    /// <summary>Builds a paged result and derives page metadata from the total item count.</summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="items">The items on the requested page.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalItems">The total number of items.</param>
    /// <returns>A populated paged result.</returns>
    public static PagedResult<T> Create<T>(
        IReadOnlyList<T> items, int page, int pageSize, int totalItems)
    {
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;
        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}
