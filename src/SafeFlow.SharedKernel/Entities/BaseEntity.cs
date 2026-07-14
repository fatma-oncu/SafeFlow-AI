using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.SharedKernel.Entities;

/// <summary>
/// Convenience base class for domain entities whose identifier is a <see cref="Guid"/>.
/// Implements <see cref="IAuditableEntity"/> and <see cref="ISoftDeletable"/> so that
/// the Infrastructure layer can automatically populate audit and soft-delete fields
/// without any per-entity boilerplate.
/// </summary>
/// <remarks>
/// All domain entities (non-aggregate-root) that belong to a single bounded context
/// should inherit from this class. Aggregate roots must instead inherit from
/// <see cref="AggregateRoot"/>.
/// </remarks>
public abstract class BaseEntity : Entity<Guid>, IAuditableEntity, ISoftDeletable
{
    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public string? CreatedBy { get; set; }

    /// <inheritdoc/>
    public DateTime? LastModifiedAt { get; set; }

    /// <inheritdoc/>
    public string? LastModifiedBy { get; set; }

    /// <inheritdoc/>
    public bool IsDeleted { get; private set; }

    /// <inheritdoc/>
    public DateTime? DeletedAt { get; private set; }

    /// <inheritdoc/>
    public string? DeletedBy { get; private set; }

    /// <summary>
    /// Marks this entity as soft-deleted.
    /// </summary>
    /// <param name="deletedBy">
    /// The identifier of the actor performing the deletion (user ID or system name).
    /// </param>
    /// <param name="deletedAt">
    /// The UTC instant of deletion. When <c>null</c>, <see cref="DateTime.UtcNow"/> is used.
    /// </param>
    public void SoftDelete(string deletedBy, DateTime? deletedAt = null)
    {
        IsDeleted = true;
        DeletedAt = deletedAt ?? DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
