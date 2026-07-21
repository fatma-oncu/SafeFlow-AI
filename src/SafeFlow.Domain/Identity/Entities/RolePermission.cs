using SafeFlow.SharedKernel.Entities;
using SafeFlow.Domain.Identity.ValueObjects;

namespace SafeFlow.Domain.Identity.Entities;

/// <summary>
/// Represents the many-to-many join between a <c>Role</c> and a <c>Permission</c>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RolePermission"/> is a child entity of the <c>Role</c> aggregate root.
/// It must only be created through <c>Role.AddPermission</c> — never constructed directly
/// by callers outside the <c>SafeFlow.Domain</c> assembly.
/// </para>
/// <para>
/// <c>Permission</c> is stored as a value object embedded within this entity.
/// The Infrastructure layer maps it using EF Core owned-entity configuration,
/// keeping the domain model free from persistence annotations.
/// </para>
/// </remarks>
public sealed class RolePermission : BaseEntity
{
    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Private parameterless constructor reserved for ORM materialisation.
    /// </summary>
    private RolePermission() { }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the identifier of the <c>Role</c> aggregate that owns this permission.
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Gets the <c>Permission</c> value object granted to the role.
    /// </summary>
    public Permission Permission { get; private set; } = default!;

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="RolePermission"/> entry.
    /// Intended for use only by the <c>Role</c> aggregate root.
    /// </summary>
    /// <param name="roleId">
    /// The identifier of the role receiving the permission.
    /// Must not be an empty <see cref="Guid"/>.
    /// </param>
    /// <param name="permission">
    /// The permission to grant. Must not be <c>null</c>.
    /// </param>
    /// <returns>A new, unsaved <see cref="RolePermission"/> entity.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="roleId"/> is an empty GUID.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="permission"/> is <c>null</c>.
    /// </exception>
    internal static RolePermission Create(Guid roleId, Permission permission)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("RoleId must not be an empty GUID.", nameof(roleId));
        ArgumentNullException.ThrowIfNull(permission, nameof(permission));

        var entity = new RolePermission();
        entity.Id = Guid.NewGuid();
        entity.RoleId = roleId;
        entity.Permission = permission;
        return entity;
    }
}
