using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/> that delegates persistence
/// to <see cref="SafeFlowDbContext"/>.
/// </summary>
/// <remarks>
/// Domain events collected on aggregate roots during the command are dispatched by
/// <see cref="SafeFlowDbContext.SaveChangesAsync"/> after the transaction is committed,
/// guaranteeing events are never raised for uncommitted state changes.
/// </remarks>
internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly SafeFlowDbContext _context;

    /// <summary>
    /// Initialises a new <see cref="UnitOfWork"/>.
    /// </summary>
    /// <param name="context">The scoped EF Core context.</param>
    public UnitOfWork(SafeFlowDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }
}
