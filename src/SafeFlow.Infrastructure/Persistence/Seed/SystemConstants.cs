namespace SafeFlow.Infrastructure.Persistence.Seed;

/// <summary>
/// Fixed identifiers and constants used by the database seed layer.
/// </summary>
/// <remarks>
/// Using deterministic (hard-coded) GUIDs for seeded entities ensures that
/// migrations and re-seeds are idempotent — the same entity is upserted, not
/// duplicated, on every startup.  These values must never be changed after the
/// first deployment; treat them as immutable schema constants.
/// </remarks>
public static class SystemConstants
{
    // ── Tenant ────────────────────────────────────────────────────────────────

    /// <summary>
    /// The tenant identifier for the built-in system tenant.
    /// All seeded users and roles belong to this tenant.
    /// </summary>
    public static readonly Guid SystemTenantId =
        new("00000000-0000-0000-0000-000000000001");

    // ── Users ─────────────────────────────────────────────────────────────────

    /// <summary>The fixed identifier for the seeded system administrator user.</summary>
    public static readonly Guid SystemAdminUserId =
        new("00000000-0000-0000-0001-000000000001");

    // ── Roles ─────────────────────────────────────────────────────────────────

    /// <summary>The fixed identifier for the <c>SuperAdmin</c> system role.</summary>
    public static readonly Guid SuperAdminRoleId =
        new("00000000-0000-0000-0002-000000000001");

    /// <summary>The fixed identifier for the <c>Admin</c> system role.</summary>
    public static readonly Guid AdminRoleId =
        new("00000000-0000-0000-0002-000000000002");

    /// <summary>The fixed identifier for the <c>Manager</c> system role.</summary>
    public static readonly Guid ManagerRoleId =
        new("00000000-0000-0000-0002-000000000003");

    /// <summary>The fixed identifier for the <c>Employee</c> system role.</summary>
    public static readonly Guid EmployeeRoleId =
        new("00000000-0000-0000-0002-000000000004");

    // ── Role names ────────────────────────────────────────────────────────────

    /// <summary>Display name of the SuperAdmin role.</summary>
    public const string SuperAdminRoleName = "SuperAdmin";

    /// <summary>Display name of the Admin role.</summary>
    public const string AdminRoleName = "Admin";

    /// <summary>Display name of the Manager role.</summary>
    public const string ManagerRoleName = "Manager";

    /// <summary>Display name of the Employee role.</summary>
    public const string EmployeeRoleName = "Employee";
}
