using SafeFlow.Domain.Identity.ValueObjects;

namespace SafeFlow.Infrastructure.Persistence.Seed;

/// <summary>
/// Canonical catalogue of all <see cref="Permission"/> values that exist in the
/// system, and the role-to-permission assignments applied at seed time.
/// </summary>
/// <remarks>
/// <para>
/// This class is the single source of truth for Infrastructure-layer permission
/// definitions. It mirrors the <c>Permissions</c> constants in the API layer.
/// When a new permission is added to the API, a corresponding entry must be
/// added here. A <c>/learn</c> note has been recorded to enforce this contract.
/// </para>
/// <para>
/// Permissions follow the pattern <c>Module:Action</c>. The <c>Permission</c>
/// domain value object uses <c>Module.Action</c> (dot) as its <see cref="Permission.CanonicalName"/>.
/// Both styles are maintained for their respective layers.
/// </para>
/// </remarks>
internal static class SystemPermissions
{
    // ── Permission definitions ────────────────────────────────────────────────

    internal static readonly Permission UsersRead   = Permission.Create("Users",   "Read");
    internal static readonly Permission UsersWrite  = Permission.Create("Users",   "Write");
    internal static readonly Permission RolesRead   = Permission.Create("Roles",   "Read");
    internal static readonly Permission RolesAssign = Permission.Create("Roles",   "Assign");
    internal static readonly Permission RolesRevoke = Permission.Create("Roles",   "Revoke");

    // ── Role-permission matrix ────────────────────────────────────────────────

    /// <summary>
    /// Returns all permissions assigned to the <c>SuperAdmin</c> role.
    /// SuperAdmin is granted every defined permission.
    /// </summary>
    internal static IReadOnlyList<Permission> SuperAdminPermissions() =>
    [
        UsersRead,
        UsersWrite,
        RolesRead,
        RolesAssign,
        RolesRevoke,
    ];

    /// <summary>
    /// Returns all permissions assigned to the <c>Admin</c> role.
    /// Admin mirrors SuperAdmin in Phase 1.
    /// </summary>
    internal static IReadOnlyList<Permission> AdminPermissions() =>
    [
        UsersRead,
        UsersWrite,
        RolesRead,
        RolesAssign,
        RolesRevoke,
    ];

    /// <summary>
    /// Returns all permissions assigned to the <c>Manager</c> role.
    /// Managers can read users and roles but cannot mutate them.
    /// </summary>
    internal static IReadOnlyList<Permission> ManagerPermissions() =>
    [
        UsersRead,
        RolesRead,
    ];

    /// <summary>
    /// Returns all permissions assigned to the <c>Employee</c> role.
    /// Employees can read their own profile only.
    /// </summary>
    internal static IReadOnlyList<Permission> EmployeePermissions() =>
    [
        UsersRead,
    ];
}
