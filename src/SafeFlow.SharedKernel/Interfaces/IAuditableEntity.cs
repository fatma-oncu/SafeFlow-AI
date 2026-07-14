namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Marks an entity as auditable — tracking creation and last-modification metadata.
/// </summary>
/// <remarks>
/// The Infrastructure layer's <c>SaveChangesAsync</c> intercept automatically
/// populates these properties using the <c>ICurrentUserService</c> and
/// <c>IDateTimeService</c> abstractions. Aggregate roots and entities that need
/// a full audit trail should implement this interface.
/// </remarks>
public interface IAuditableEntity
{
    /// <summary>Gets the UTC instant at which this entity was first created.</summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Gets the identifier (user ID or system name) of the actor who created
    /// this entity.
    /// </summary>
    string? CreatedBy { get; }

    /// <summary>
    /// Gets the UTC instant at which this entity was last modified, or
    /// <c>null</c> if it has never been modified after creation.
    /// </summary>
    DateTime? LastModifiedAt { get; }

    /// <summary>
    /// Gets the identifier (user ID or system name) of the actor who last
    /// modified this entity, or <c>null</c> if it has never been modified.
    /// </summary>
    string? LastModifiedBy { get; }
}
