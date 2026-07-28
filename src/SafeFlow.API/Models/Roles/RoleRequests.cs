namespace SafeFlow.API.Models.Roles;

/// <summary>Request body for <c>POST /api/v1/users/{id}/roles</c>.</summary>
public sealed record AssignRoleRequest(Guid RoleId);
