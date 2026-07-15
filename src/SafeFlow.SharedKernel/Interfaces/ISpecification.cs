using System.Linq.Expressions;

namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Defines the contract for a Specification — an encapsulated, composable query
/// predicate and its associated fetch behaviour options.
/// </summary>
/// <typeparam name="T">
/// The type of the entity this specification applies to.
/// </typeparam>
/// <remarks>
/// <para>
/// The Specification pattern (Evans, 2003) moves query logic out of repositories
/// and into named, reusable, testable objects.  Instead of
/// <c>repository.GetActiveEmployeesForTenant(tenantId)</c>, callers create an
/// <c>ActiveEmployeesForTenantSpecification(tenantId)</c> and pass it to the
/// repository's generic <c>ListAsync</c> or <c>FirstOrDefaultAsync</c> methods.
/// </para>
/// <para>
/// This interface carries only <em>data</em> — expressions and flags.  No
/// expression compilation, EF Core references, or <c>IQueryable</c> appear here.
/// The Infrastructure layer's EF Core specification evaluator reads the expressions
/// and applies them to the <c>IQueryable</c> pipeline, keeping all query execution
/// details inside Infrastructure.
/// </para>
/// <para>
/// Both expression-based includes (<see cref="Includes"/>) and string-based includes
/// (<see cref="IncludeStrings"/>) are supported to accommodate both navigation
/// property expressions and multi-level string paths.
/// </para>
/// </remarks>
public interface ISpecification<T>
{
    // -------------------------------------------------------------------------
    // Filter
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the filter predicate applied to the entity set, or <c>null</c> when no
    /// filter is applied (all entities are returned).
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    // -------------------------------------------------------------------------
    // Eager loading
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the list of navigation property expressions to eager-load using
    /// <c>Include()</c>.  Empty when no eager loading is required.
    /// </summary>
    IReadOnlyList<Expression<Func<T, object?>>> Includes { get; }

    /// <summary>
    /// Gets the list of string-based include paths for multi-level eager loading
    /// (e.g., <c>"Department.Manager"</c>).  Empty when no string includes are required.
    /// </summary>
    IReadOnlyList<string> IncludeStrings { get; }

    // -------------------------------------------------------------------------
    // Ordering
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the expression used for ascending ordering, or <c>null</c> when no
    /// ordering is specified.
    /// </summary>
    Expression<Func<T, object?>>? OrderBy { get; }

    /// <summary>
    /// Gets the expression used for descending ordering, or <c>null</c> when no
    /// ordering is specified.
    /// </summary>
    /// <remarks>
    /// At most one of <see cref="OrderBy"/> and <see cref="OrderByDescending"/>
    /// should be non-<c>null</c>. When both are set, <see cref="OrderBy"/> takes
    /// precedence; the Infrastructure evaluator ignores <see cref="OrderByDescending"/>.
    /// </remarks>
    Expression<Func<T, object?>>? OrderByDescending { get; }

    // -------------------------------------------------------------------------
    // Paging
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the number of records to skip, or <c>null</c> when paging is not applied.
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Gets the maximum number of records to return, or <c>null</c> when paging is
    /// not applied.
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Gets a value indicating whether paging (skip/take) is enabled for this
    /// specification.
    /// </summary>
    bool IsPagingEnabled { get; }

    // -------------------------------------------------------------------------
    // EF Core query hints (interpreted by the Infrastructure evaluator)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the Infrastructure evaluator should apply
    /// <c>AsNoTracking()</c> to the resulting query.
    /// Use <c>true</c> for read-only projections to improve performance.
    /// </summary>
    bool AsNoTracking { get; }

    /// <summary>
    /// Gets a value indicating whether the Infrastructure evaluator should apply
    /// <c>IgnoreQueryFilters()</c>, bypassing global query filters such as
    /// soft-delete and tenant-scoping filters.
    /// </summary>
    /// <remarks>
    /// This flag must be used only in administrative or system-level contexts
    /// where cross-tenant or deleted-entity access is intentionally required.
    /// </remarks>
    bool IgnoreQueryFilters { get; }

    /// <summary>
    /// Gets a value indicating whether the Infrastructure evaluator should apply
    /// <c>AsSplitQuery()</c>, splitting the generated SQL into multiple queries
    /// to avoid the cartesian explosion problem with multiple collection includes.
    /// </summary>
    bool SplitQuery { get; }
}
