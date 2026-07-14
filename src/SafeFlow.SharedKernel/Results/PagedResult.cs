namespace SafeFlow.SharedKernel.Results;

/// <summary>
/// Represents a single page of a larger result set returned by a paginated query.
/// </summary>
/// <typeparam name="T">The type of each item in the result set.</typeparam>
/// <remarks>
/// <para>
/// Application query handlers produce a <see cref="PagedResult{T}"/> by calling
/// <see cref="Create"/> after fetching the requested slice from the repository.
/// The handler is responsible for passing the total record count so that the
/// caller can compute navigation metadata.
/// </para>
/// <para>
/// All properties are read-only after construction, making this type safe to
/// cache and share across threads.
/// </para>
/// </remarks>
public sealed class PagedResult<T>
{
    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    private PagedResult(
        IReadOnlyList<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        if (page < 1)
            throw new ArgumentOutOfRangeException(
                nameof(page), page, "Page number must be greater than or equal to 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(
                nameof(pageSize), pageSize, "Page size must be between 1 and 100 inclusive.");

        if (totalCount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(totalCount), totalCount, "Total count must be non-negative.");

        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>Gets the items on the current page.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Gets the total number of records across all pages.</summary>
    public int TotalCount { get; }

    /// <summary>Gets the 1-based current page number.</summary>
    public int Page { get; }

    /// <summary>Gets the maximum number of items per page.</summary>
    public int PageSize { get; }

    /// <summary>
    /// Gets the total number of pages required to display all records.
    /// Returns <c>0</c> when <see cref="TotalCount"/> is zero.
    /// </summary>
    public int TotalPages =>
        TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Gets a value indicating whether a next page exists.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets a value indicating whether a previous page exists.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="PagedResult{T}"/> from a repository slice.
    /// </summary>
    /// <param name="items">
    /// The items on the current page. Must not be <c>null</c>.
    /// </param>
    /// <param name="totalCount">
    /// The total number of records matching the query across all pages.
    /// Must be non-negative.
    /// </param>
    /// <param name="page">The 1-based page number. Must be &gt;= 1.</param>
    /// <param name="pageSize">
    /// The maximum items per page. Must be between 1 and 100 inclusive.
    /// </param>
    /// <returns>A new <see cref="PagedResult{T}"/> instance.</returns>
    public static PagedResult<T> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int page,
        int pageSize) =>
        new(items, totalCount, page, pageSize);

    /// <summary>
    /// Creates an empty <see cref="PagedResult{T}"/> for the given page parameters.
    /// </summary>
    /// <param name="page">The 1-based page number. Must be &gt;= 1.</param>
    /// <param name="pageSize">
    /// The maximum items per page. Must be between 1 and 100 inclusive.
    /// </param>
    /// <returns>A <see cref="PagedResult{T}"/> with an empty items list.</returns>
    public static PagedResult<T> Empty(int page = 1, int pageSize = 20) =>
        new(Array.Empty<T>(), 0, page, pageSize);
}
