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

    internal static Permission UsersRead   => Permission.Create("Users",   "Read");
    internal static Permission UsersWrite  => Permission.Create("Users",   "Write");
    internal static Permission RolesRead   => Permission.Create("Roles",   "Read");
    internal static Permission RolesAssign => Permission.Create("Roles",   "Assign");
    internal static Permission RolesRevoke => Permission.Create("Roles",   "Revoke");

    internal static Permission EmployeesRead     => Permission.Create("Employees", "Read");
    internal static Permission EmployeesCreate   => Permission.Create("Employees", "Create");
    internal static Permission EmployeesUpdate   => Permission.Create("Employees", "Update");
    internal static Permission EmployeesDelete   => Permission.Create("Employees", "Delete");
    internal static Permission EmployeesTransfer => Permission.Create("Employees", "Transfer");

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
        EmployeesRead,
        EmployeesCreate,
        EmployeesUpdate,
        EmployeesDelete,
        EmployeesTransfer,
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
        EmployeesRead,
        EmployeesCreate,
        EmployeesUpdate,
        EmployeesDelete,
        EmployeesTransfer,
    ];

    /// <summary>
    /// Returns all permissions assigned to the <c>Manager</c> role.
    /// Managers can read users, roles and employees, create/update/transfer employees.
    /// </summary>
    internal static IReadOnlyList<Permission> ManagerPermissions() =>
    [
        UsersRead,
        RolesRead,
        EmployeesRead,
        EmployeesCreate,
        EmployeesUpdate,
        EmployeesTransfer,
    ];

    /// <summary>
    /// Returns all permissions assigned to the <c>Employee</c> role.
    /// Employees can read profiles.
    /// </summary>
    internal static IReadOnlyList<Permission> EmployeePermissions() =>
    [
        UsersRead,
        EmployeesRead,
    ];
}
