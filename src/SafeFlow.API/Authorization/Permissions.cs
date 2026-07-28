namespace SafeFlow.API.Authorization;

/// <summary>
/// Canonical permission strings used by endpoint <c>[Authorize(Policy = ...)]</c>
/// attributes throughout the API layer.
/// </summary>
/// <remarks>
/// Permissions follow the pattern <c>Module:Action</c>, matching the
/// <c>Permission</c> value object in the Domain layer.
/// These constants are the sole source of truth — no magic strings elsewhere.
/// </remarks>
public static class Permissions
{
    // ── Users ────────────────────────────────────────────────────────────────
    public const string UsersRead    = "Users:Read";
    public const string UsersWrite   = "Users:Write";

    // ── Roles ────────────────────────────────────────────────────────────────
    public const string RolesRead    = "Roles:Read";
    public const string RolesAssign  = "Roles:Assign";
    public const string RolesRevoke  = "Roles:Revoke";
}
