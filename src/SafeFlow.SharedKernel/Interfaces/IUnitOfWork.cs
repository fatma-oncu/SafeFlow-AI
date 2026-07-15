namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Defines the boundary of a single unit of work, grouping one or more repository
/// operations into an atomic, consistent transaction.
/// </summary>
/// <remarks>
/// <para>
/// The Unit of Work pattern ensures that all changes made to aggregates within a
/// single command handler are committed atomically — either all succeed or all are
/// rolled back.  Application-layer command handlers depend on this interface rather
/// than on EF Core's <c>DbContext</c> directly, preserving Clean Architecture
/// boundaries.
/// </para>
/// <para>
/// Domain events collected on aggregate roots during the command are dispatched by
/// the Infrastructure implementation of <see cref="IUnitOfWork"/> immediately after
/// <see cref="SaveChangesAsync"/> commits the transaction, guaranteeing that events
/// are never raised for uncommitted state changes.
/// </para>
/// <para>
/// Implementations must be registered with a scoped lifetime so that all repositories
/// resolved within a single request share the same underlying transaction context.
/// </para>
/// </remarks>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    /// Persists all pending changes accumulated within the current unit of work to the
    /// underlying store and dispatches any domain events raised by aggregate roots.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous save operation.  The task result is the
    /// number of state entries written to the underlying store.
    /// </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
