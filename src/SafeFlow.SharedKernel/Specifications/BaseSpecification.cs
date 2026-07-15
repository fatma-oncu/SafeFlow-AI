using System.Linq.Expressions;
using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.SharedKernel.Specifications;

/// <summary>
/// Abstract base class that provides a fluent, protected API for building
/// <see cref="ISpecification{T}"/> instances.
/// </summary>
/// <typeparam name="T">
/// The entity type this specification applies to.
/// </typeparam>
/// <remarks>
/// <para>
/// Concrete specifications inherit from <see cref="BaseSpecification{T}"/> and call
/// the protected builder methods (<see cref="AddCriteria"/>, <c>AddInclude</c>,
/// <see cref="ApplyOrderBy"/>, <see cref="ApplyPaging"/>, etc.) from their
/// constructors.  This ensures that every specification object is fully configured at
/// construction time and is immutable thereafter.
/// </para>
/// <para>
/// Example:
/// <code>
/// public sealed class ActiveEmployeesForTenantSpec : BaseSpecification&lt;Employee&gt;
/// {
///     public ActiveEmployeesForTenantSpec(Guid tenantId)
///         : base(e =&gt; e.TenantId == tenantId &amp;&amp; !e.IsDeleted)
///     {
///         AddInclude(e =&gt; e.Department);
///         ApplyOrderBy(e =&gt; e.LastName);
///         ApplyNoTracking();
///     }
/// }
/// </code>
/// </para>
/// </remarks>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, object?>>> _includes = [];
    private readonly List<string> _includeStrings = [];

    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a <see cref="BaseSpecification{T}"/> with no filter predicate.
    /// All entities of type <c>T</c> will be returned unless a criteria expression
    /// is added by the subclass constructor.
    /// </summary>
    protected BaseSpecification()
    {
    }

    /// <summary>
    /// Initializes a <see cref="BaseSpecification{T}"/> with the given filter
    /// <paramref name="criteria"/>.
    /// </summary>
    /// <param name="criteria">
    /// The filter predicate applied to the entity set.
    /// Must not be <c>null</c>.
    /// </param>
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria, nameof(criteria));
        Criteria = criteria;
    }

    // -------------------------------------------------------------------------
    // ISpecification<T> — properties
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<Expression<Func<T, object?>>> Includes => _includes.AsReadOnly();

    /// <inheritdoc/>
    public IReadOnlyList<string> IncludeStrings => _includeStrings.AsReadOnly();

    /// <inheritdoc/>
    public Expression<Func<T, object?>>? OrderBy { get; private set; }

    /// <inheritdoc/>
    public Expression<Func<T, object?>>? OrderByDescending { get; private set; }

    /// <inheritdoc/>
    public int? Skip { get; private set; }

    /// <inheritdoc/>
    public int? Take { get; private set; }

    /// <inheritdoc/>
    public bool IsPagingEnabled { get; private set; }

    /// <inheritdoc/>
    public bool AsNoTracking { get; private set; }

    /// <inheritdoc/>
    public bool IgnoreQueryFilters { get; private set; }

    /// <inheritdoc/>
    public bool SplitQuery { get; private set; }

    // -------------------------------------------------------------------------
    // Protected builder methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sets or replaces the filter predicate for this specification.
    /// </summary>
    /// <param name="criteria">The filter predicate.  Must not be <c>null</c>.</param>
    protected void AddCriteria(Expression<Func<T, bool>> criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria, nameof(criteria));
        Criteria = criteria;
    }

    /// <summary>
    /// Adds a navigation property expression to the eager-load list.
    /// </summary>
    /// <param name="includeExpression">
    /// The navigation property selector (e.g., <c>e =&gt; e.Department</c>).
    /// Must not be <c>null</c>.
    /// </param>
    protected void AddInclude(Expression<Func<T, object?>> includeExpression)
    {
        ArgumentNullException.ThrowIfNull(includeExpression, nameof(includeExpression));
        _includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds a string-based include path for multi-level eager loading
    /// (e.g., <c>"Department.Manager"</c>).
    /// </summary>
    /// <param name="includeString">
    /// The dot-separated navigation path.  Must not be <c>null</c> or whitespace.
    /// </param>
    protected void AddInclude(string includeString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(includeString, nameof(includeString));
        _includeStrings.Add(includeString);
    }

    /// <summary>
    /// Applies ascending ordering to the query.
    /// </summary>
    /// <param name="orderByExpression">
    /// The property selector for ascending sort.  Must not be <c>null</c>.
    /// </param>
    protected void ApplyOrderBy(Expression<Func<T, object?>> orderByExpression)
    {
        ArgumentNullException.ThrowIfNull(orderByExpression, nameof(orderByExpression));
        OrderBy = orderByExpression;
        OrderByDescending = null;
    }

    /// <summary>
    /// Applies descending ordering to the query.
    /// </summary>
    /// <param name="orderByDescendingExpression">
    /// The property selector for descending sort.  Must not be <c>null</c>.
    /// </param>
    protected void ApplyOrderByDescending(Expression<Func<T, object?>> orderByDescendingExpression)
    {
        ArgumentNullException.ThrowIfNull(orderByDescendingExpression, nameof(orderByDescendingExpression));
        OrderByDescending = orderByDescendingExpression;
        OrderBy = null;
    }

    /// <summary>
    /// Applies skip/take paging to the query and sets <see cref="IsPagingEnabled"/>
    /// to <c>true</c>.
    /// </summary>
    /// <param name="skip">
    /// The number of records to skip.  Must be non-negative.
    /// </param>
    /// <param name="take">
    /// The maximum number of records to return.  Must be greater than zero.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="skip"/> is negative or <paramref name="take"/> is
    /// less than one.
    /// </exception>
    protected void ApplyPaging(int skip, int take)
    {
        if (skip < 0)
            throw new ArgumentOutOfRangeException(nameof(skip), skip, "Skip must be non-negative.");
        if (take < 1)
            throw new ArgumentOutOfRangeException(nameof(take), take, "Take must be greater than zero.");

        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    /// <summary>
    /// Instructs the Infrastructure evaluator to apply <c>AsNoTracking()</c> for
    /// improved read performance.  Use for all read-only query specifications.
    /// </summary>
    protected void ApplyNoTracking() => AsNoTracking = true;

    /// <summary>
    /// Instructs the Infrastructure evaluator to bypass global query filters
    /// (soft-delete, tenant-scoping).  Use only in administrative or system contexts.
    /// </summary>
    protected void ApplyIgnoreQueryFilters() => IgnoreQueryFilters = true;

    /// <summary>
    /// Instructs the Infrastructure evaluator to apply <c>AsSplitQuery()</c> to
    /// avoid cartesian explosion when multiple collection navigations are included.
    /// </summary>
    protected void ApplySplitQuery() => SplitQuery = true;
}
