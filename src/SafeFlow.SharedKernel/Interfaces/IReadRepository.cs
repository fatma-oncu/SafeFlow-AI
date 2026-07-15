namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Defines read-only data access operations for entities of type
/// <typeparamref name="TEntity"/>.
/// </summary>
/// <typeparam name="TEntity">
/// The entity type this repository operates on.  Must be a non-null reference type.
/// </typeparam>
/// <remarks>
/// <para>
/// <see cref="IReadRepository{TEntity}"/> exposes the query side of the repository
/// contract and is consumed by CQRS query handlers.  Query handlers depend only on
/// this leaner interface, not on the full <see cref="IRepository{TEntity}"/> that
/// includes mutation methods, following the Interface Segregation Principle.
/// </para>
/// <para>
/// All methods are asynchronous and accept a <see cref="CancellationToken"/> to
/// support cooperative cancellation in ASP.NET Core request pipelines and background
/// jobs.
/// </para>
/// <para>
/// The Infrastructure layer implements this interface using EF Core without exposing
/// any EF Core type to the Application or Domain layers.
/// </para>
/// </remarks>
public interface IReadRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Retrieves the entity with the specified <paramref name="id"/>, or <c>null</c>
    /// if no matching entity exists.
    /// </summary>
    /// <param name="id">The primary key (Guid) of the entity to retrieve.</param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task whose result is the matching entity, or <c>null</c> if not found.
    /// </returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the first entity that satisfies the given <paramref name="specification"/>,
    /// or <c>null</c> if no match is found.
    /// </summary>
    /// <param name="specification">
    /// The specification defining the filter, ordering, and fetch options.
    /// Must not be <c>null</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task whose result is the first matching entity, or <c>null</c> if none satisfy
    /// the specification.
    /// </returns>
    Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entities in the repository as an immutable list.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task whose result is a read-only list of all entities.  Returns an empty list
    /// when no entities exist.
    /// </returns>
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entities that satisfy the given <paramref name="specification"/>
    /// as an immutable list.
    /// </summary>
    /// <param name="specification">
    /// The specification defining the filter, includes, ordering, and paging options.
    /// Must not be <c>null</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task whose result is a read-only list of matching entities.  Returns an empty
    /// list when no entities satisfy the specification.
    /// </returns>
    Task<IReadOnlyList<TEntity>> ListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any entity satisfies the given <paramref name="specification"/>.
    /// </summary>
    /// <param name="specification">
    /// The specification containing the filter predicate.  Must not be <c>null</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task whose result is <c>true</c> if at least one entity satisfies the
    /// specification; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the number of entities that satisfy the given
    /// <paramref name="specification"/>.
    /// </summary>
    /// <param name="specification">
    /// The specification containing the filter predicate.  Must not be <c>null</c>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task whose result is the count of matching entities.
    /// </returns>
    Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);
}
