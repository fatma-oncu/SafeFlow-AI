namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Marks an entity as supporting soft deletion.
/// </summary>
/// <remarks>
/// Entities implementing this interface are never physically removed from the
/// database. Instead, <see cref="IsDeleted"/> is set to <c>true</c> and
/// <see cref="DeletedAt"/> is recorded. The Infrastructure layer's EF Core
/// Global Query Filter excludes soft-deleted rows from all standard queries.
/// Hard deletion must be performed explicitly and only by administrative operations.
/// </remarks>
public interface ISoftDeletable
{
    /// <summary>Gets a value indicating whether this entity has been soft-deleted.</summary>
    bool IsDeleted { get; }

    /// <summary>
    /// Gets the UTC instant at which this entity was soft-deleted, or
    /// <c>null</c> if the entity has not been deleted.
    /// </summary>
    DateTime? DeletedAt { get; }

    /// <summary>
    /// Gets the identifier (user ID or system name) of the actor who performed
    /// the soft deletion, or <c>null</c> if the entity has not been deleted.
    /// </summary>
    string? DeletedBy { get; }
}
