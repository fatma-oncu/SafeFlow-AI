namespace SafeFlow.SharedKernel.Results;

/// <summary>
/// Provides the common pagination parameters for list-based queries.
/// </summary>
/// <remarks>
/// <para>
/// Concrete query classes that return paginated data must inherit from this class
/// and pass the caller-supplied page and page-size values to the base constructor.
/// </para>
/// <para>
/// Validation rules (invariants):
/// <list type="bullet">
///   <item><see cref="Page"/> must be &gt;= 1.</item>
///   <item><see cref="PageSize"/> must be between 1 and 100 inclusive.</item>
/// </list>
/// These rules are enforced at construction time so that no handler ever receives
/// an out-of-range query.
/// </para>
/// </remarks>
public abstract class PagedQuery
{
    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes pagination parameters for a list query.
    /// </summary>
    /// <param name="page">The 1-based page number. Must be &gt;= 1.</param>
    /// <param name="pageSize">
    /// The maximum number of items per page. Must be between 1 and 100 inclusive.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="page"/> is less than 1, or when
    /// <paramref name="pageSize"/> is less than 1 or greater than 100.
    /// </exception>
    protected PagedQuery(int page = 1, int pageSize = 20)
    {
        if (page < 1)
            throw new ArgumentOutOfRangeException(
                nameof(page), page, "Page number must be greater than or equal to 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentOutOfRangeException(
                nameof(pageSize), pageSize, "Page size must be between 1 and 100 inclusive.");

        Page = page;
        PageSize = pageSize;
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the 1-based page number requested by the caller.
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Gets the maximum number of items to return per page.
    /// Minimum: 1 — Maximum: 100.
    /// </summary>
    public int PageSize { get; }
}
