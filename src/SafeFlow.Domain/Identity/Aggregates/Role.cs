using SafeFlow.SharedKernel.Entities;
using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.Domain.Identity.Entities;
using SafeFlow.Domain.Identity.ValueObjects;

namespace SafeFlow.Domain.Identity.Aggregates;

/// <summary>
/// Represents an authorisation role that groups a named set of permissions.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Role"/> is an aggregate root in the Identity bounded context.
/// All mutations to the role (adding or removing permissions) must occur through
/// its public domain methods to preserve invariants and raise domain events.
/// </para>
/// <para>
/// System roles (<see cref="IsSystemRole"/> = <c>true</c>) are seeded by the
/// application and should not be deleted or renamed through normal user flows.
/// Application-layer command handlers are responsible for enforcing this guard.
/// </para>
/// <para>
/// <see cref="RolePermission"/> child entities are managed by this aggregate.
/// Duplicate permissions are silently ignored (idempotent assignment).
/// </para>
/// </remarks>
public sealed class Role : AggregateRoot
{
    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    private readonly List<RolePermission> _rolePermissions = [];

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Private parameterless constructor reserved for ORM materialisation.
    /// </summary>
    private Role() { }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the unique display name of this role (e.g., <c>"Company Administrator"</c>).
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets an optional human-readable description of what this role grants access to.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this role is a built-in system role.
    /// System roles are seeded at startup and are protected from deletion via the
    /// application layer.
    /// </summary>
    public bool IsSystemRole { get; private set; }

    /// <summary>
    /// Gets the read-only set of permissions assigned to this role.
    /// </summary>
    public IReadOnlyCollection<RolePermission> RolePermissions =>
        _rolePermissions.AsReadOnly();

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="Role"/> aggregate.
    /// </summary>
    /// <param name="id">The unique identifier for this role.</param>
    /// <param name="name">
    /// The display name of the role.
    /// Must not be <c>null</c> or whitespace and must not exceed 100 characters.
    /// </param>
    /// <param name="description">
    /// An optional human-readable description. Must not exceed 500 characters when provided.
    /// </param>
    /// <param name="isSystemRole">
    /// <c>true</c> if this is a built-in system role; <c>false</c> for tenant-defined roles.
    /// </param>
    /// <returns>A new <see cref="Role"/> aggregate root instance.</returns>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="name"/> is invalid or <paramref name="description"/>
    /// exceeds the maximum length.
    /// </exception>
    public static Role Create(
        Guid id,
        string name,
        string? description = null,
        bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException(nameof(name), "Role name must not be empty.");

        var trimName = name.Trim();
        if (trimName.Length > 100)
            throw new ValidationException(nameof(name), "Role name must not exceed 100 characters.");

        if (description is not null && description.Length > 500)
            throw new ValidationException(nameof(description), "Role description must not exceed 500 characters.");

        var role = new Role();
        role.Id = id;
        role.Name = trimName;
        role.Description = description?.Trim();
        role.IsSystemRole = isSystemRole;
        return role;
    }

    // -------------------------------------------------------------------------
    // Domain methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Grants a <see cref="Permission"/> to this role.  Idempotent: adding a
    /// permission that is already present has no effect.
    /// </summary>
    /// <param name="permission">The permission to grant. Must not be <c>null</c>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="permission"/> is <c>null</c>.
    /// </exception>
    public void AddPermission(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission, nameof(permission));

        bool alreadyGranted = _rolePermissions.Exists(
            rp => rp.Permission == permission);

        if (alreadyGranted) return;

        _rolePermissions.Add(RolePermission.Create(Id, permission));
    }

    /// <summary>
    /// Revokes a <see cref="Permission"/> from this role.  Idempotent: removing a
    /// permission that is not present has no effect.
    /// </summary>
    /// <param name="permission">The permission to revoke. Must not be <c>null</c>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="permission"/> is <c>null</c>.
    /// </exception>
    public void RemovePermission(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission, nameof(permission));

        var entry = _rolePermissions.Find(rp => rp.Permission == permission);
        if (entry is null) return;

        _rolePermissions.Remove(entry);
    }

    /// <summary>
    /// Updates the display name of this role.
    /// </summary>
    /// <param name="newName">
    /// The new role name. Must not be <c>null</c> or whitespace and must not exceed
    /// 100 characters.
    /// </param>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="newName"/> is invalid.
    /// </exception>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ValidationException(nameof(newName), "Role name must not be empty.");

        var trimmed = newName.Trim();
        if (trimmed.Length > 100)
            throw new ValidationException(nameof(newName), "Role name must not exceed 100 characters.");

        Name = trimmed;
    }
}
