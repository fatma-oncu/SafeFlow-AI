namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Defines full read-write data access operations for entities of type
/// <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type this repository operates on.  Must be a non-null reference type.
/// </typeparam>
/// <remarks>
/// <para>
/// <see cref="IRepository{TEntity}"/> extends <see cref="IReadRepository{TEntity}"/>
/// with mutation operations (add, update, delete) and is consumed by CQRS command
/// handlers.  By inheriting the read contract, a single Infrastructure implementation
/// can satisfy both read and write scenarios without code duplication.
/// </para>
/// <para>
/// <see cref="Update"/> and <see cref="Delete"/> are synchronous because EF Core
/// change-tracking operations are CPU-bound in-memory actions; they do not involve
/// I/O.  The actual database write occurs when
/// <see cref="IUnitOfWork.SaveChangesAsync"/> is called.
/// </para>
/// <para>
/// Callers must not call <c>SaveChanges</c> directly on the repository.  All
/// persistence is coordinated through <see cref="IUnitOfWork"/> to ensure atomicity
/// across multiple repository operations within a single command handler.
/// </para>
/// </remarks>
public interface IRepository<TEntity> : IReadRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Adds a new entity to the repository.  The entity is not persisted until
    /// <see cref="IUnitOfWork.SaveChangesAsync"/> is called.
    /// </summary>
    /// <param name="entity">The entity to add.  Must not be <c>null</c>.</param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the given entity as modified so that its changes are included in the
    /// next call to <see cref="IUnitOfWork.SaveChangesAsync"/>.
    /// </summary>
    /// <param name="entity">
    /// The entity instance with updated property values.  Must not be <c>null</c>.
    /// </param>
    /// <remarks>
    /// When using EF Core change tracking, calling <see cref="Update"/> is only
    /// necessary for detached entities.  Entities retrieved within the same unit
    /// of work are tracked automatically and do not require an explicit call.
    /// </remarks>
    void Update(TEntity entity);

    /// <summary>
    /// Marks the given entity for deletion so that it is removed in the next call to
    /// <see cref="IUnitOfWork.SaveChangesAsync"/>.
    /// </summary>
    /// <param name="entity">The entity to delete.  Must not be <c>null</c>.</param>
    /// <remarks>
    /// For entities that implement <see cref="ISoftDeletable"/>, the Infrastructure
    /// implementation should intercept this call and perform a soft-delete instead of
    /// a physical removal, unless the caller has explicitly opted in to hard deletion.
    /// </remarks>
    void Delete(TEntity entity);
}
