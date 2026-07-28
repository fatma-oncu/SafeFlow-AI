using Microsoft.EntityFrameworkCore;
using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic EF Core implementation of <see cref="IRepository{TEntity}"/> and
/// <see cref="IReadRepository{TEntity}"/>.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type. Must be a non-null reference type registered with the DbContext.
/// </typeparam>
/// <remarks>
/// <para>
/// No business logic lives inside this class. All filtering, ordering, and paging
/// are expressed through <see cref="ISpecification{TEntity}"/> objects and
/// translated by <see cref="SpecificationEvaluator"/>.
/// </para>
/// <para>
/// Callers must commit changes through <see cref="IUnitOfWork.SaveChangesAsync"/>
/// rather than calling <c>SaveChanges</c> directly on the context.
/// </para>
/// </remarks>
internal sealed class EfRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    private readonly SafeFlowDbContext _context;

    /// <summary>
    /// Initialises a new <see cref="EfRepository{TEntity}"/>.
    /// </summary>
    /// <param name="context">The EF Core context scoped to the current request.</param>
    public EfRepository(SafeFlowDbContext context)
    {
        _context = context;
    }

    // ── IReadRepository<TEntity> ──────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator
            .GetQuery(_context.Set<TEntity>().AsQueryable(), specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TEntity>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<TEntity>()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator
            .GetQuery(_context.Set<TEntity>().AsQueryable(), specification)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator
            .GetQuery(_context.Set<TEntity>().AsQueryable(), specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator
            .GetQuery(_context.Set<TEntity>().AsQueryable(), specification)
            .CountAsync(cancellationToken);
    }

    // ── IRepository<TEntity> ─────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc/>
    public void Update(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
    }

    /// <inheritdoc/>
    public void Delete(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
    }
}
