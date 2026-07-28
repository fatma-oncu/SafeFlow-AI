using Microsoft.EntityFrameworkCore;
using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.Infrastructure.Persistence;

/// <summary>
/// Translates an <see cref="ISpecification{T}"/> into an EF Core <see cref="IQueryable{T}"/>
/// by applying criteria, includes, ordering, paging, and query hints.
/// </summary>
/// <remarks>
/// <para>
/// The evaluator is stateless and thread-safe. It applies specification properties in
/// a deterministic order: filters → includes → ordering → paging → query hints.
/// </para>
/// <para>
/// No business logic exists here — the evaluator is a pure mechanical translation layer.
/// </para>
/// </remarks>
internal static class SpecificationEvaluator
{
    /// <summary>
    /// Applies all specification properties to the given <paramref name="inputQuery"/> and
    /// returns the resulting queryable.
    /// </summary>
    /// <typeparam name="TEntity">The entity type being queried.</typeparam>
    /// <param name="inputQuery">The base <see cref="IQueryable{TEntity}"/> to decorate.</param>
    /// <param name="specification">The specification whose properties are applied.</param>
    /// <returns>A fully decorated <see cref="IQueryable{TEntity}"/>.</returns>
    internal static IQueryable<TEntity> GetQuery<TEntity>(
        IQueryable<TEntity> inputQuery,
        ISpecification<TEntity> specification)
        where TEntity : class
    {
        var query = inputQuery;

        // ── 1. Global query filter bypass ────────────────────────────────────
        if (specification.IgnoreQueryFilters)
        {
            query = query.IgnoreQueryFilters();
        }

        // ── 2. No-tracking hint ───────────────────────────────────────────────
        if (specification.AsNoTracking)
        {
            query = query.AsNoTracking();
        }

        // ── 3. Split query hint ───────────────────────────────────────────────
        if (specification.SplitQuery)
        {
            query = query.AsSplitQuery();
        }

        // ── 4. Filter predicate ───────────────────────────────────────────────
        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        // ── 5. Expression-based includes ──────────────────────────────────────
        query = specification.Includes
            .Aggregate(query, (current, include) => current.Include(include));

        // ── 6. String-based includes ──────────────────────────────────────────
        query = specification.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // ── 7. Ordering ───────────────────────────────────────────────────────
        if (specification.OrderBy is not null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // ── 8. Paging ─────────────────────────────────────────────────────────
        if (specification.IsPagingEnabled)
        {
            query = query
                .Skip(specification.Skip!.Value)
                .Take(specification.Take!.Value);
        }

        return query;
    }
}
