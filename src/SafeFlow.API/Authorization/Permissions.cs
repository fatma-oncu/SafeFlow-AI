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

    // ── Employees ────────────────────────────────────────────────────────────
    public const string EmployeesRead     = "Employees:Read";
    public const string EmployeesCreate   = "Employees:Create";
    public const string EmployeesUpdate   = "Employees:Update";
    public const string EmployeesDelete   = "Employees:Delete";
    public const string EmployeesTransfer = "Employees:Transfer";

    // ── Risk Assessments ─────────────────────────────────────────────────────
    public const string RiskRead    = "Risk:Read";
    public const string RiskCreate  = "Risk:Create";
    public const string RiskUpdate  = "Risk:Update";
    public const string RiskDelete  = "Risk:Delete";
    public const string RiskArchive = "Risk:Archive";
    public const string RiskApprove = "Risk:Approve";

    // ── Incidents ────────────────────────────────────────────────────────────
    public const string IncidentRead        = "Incident:Read";
    public const string IncidentCreate      = "Incident:Create";
    public const string IncidentUpdate      = "Incident:Update";
    public const string IncidentDelete      = "Incident:Delete";
    public const string IncidentAssign      = "Incident:Assign";
    public const string IncidentInvestigate = "Incident:Investigate";
    public const string IncidentResolve     = "Incident:Resolve";
    public const string IncidentClose       = "Incident:Close";
}
