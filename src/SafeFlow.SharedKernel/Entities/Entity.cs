namespace SafeFlow.SharedKernel.Entities;

/// <summary>
/// Generic base class for all domain entities. Provides identity-based structural
/// equality semantics according to DDD principles.
/// </summary>
/// <typeparam name="TId">
/// The type of the entity's identifier. Must be a non-nullable value or reference
/// type (e.g., <see cref="Guid"/>, <c>int</c>, or a strongly-typed ID record).
/// </typeparam>
/// <remarks>
/// Two entities are considered equal if and only if they share the same runtime
/// type and the same non-default identifier. Entities with a default identifier
/// value are never considered equal to any other entity, even themselves, to
/// prevent unintentional equality of unsaved (transient) entities.
/// </remarks>
public abstract class Entity<TId> where TId : notnull
{
    /// <summary>
    /// Gets the unique identifier of this entity within its aggregate or repository.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Determines whether this entity is equal to another object.
    /// Two entities are equal if they share the same runtime type and the same
    /// non-default <see cref="Id"/>.
    /// </summary>
    /// <param name="obj">The object to compare with this entity.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="obj"/> is the same type and has the same
    /// non-default identifier; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id.Equals(default(TId)) || other.Id.Equals(default(TId)))
            return false;

        return Id.Equals(other.Id);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <summary>
    /// Returns <c>true</c> when both entities are equal according to
    /// <see cref="Equals(object?)"/>.
    /// </summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>Returns <c>true</c> when the two entities are not equal.</summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}
