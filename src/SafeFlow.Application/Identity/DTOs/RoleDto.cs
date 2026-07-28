namespace SafeFlow.Application.Identity.DTOs;

/// <summary>
/// A read-only projection of a <c>Role</c> aggregate suitable for external consumption.
/// </summary>
public sealed record RoleDto
{
    /// <summary>Gets the role's unique identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the role's unique display name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets the optional human-readable description of this role.</summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets a value indicating whether this role is a built-in system role.
    /// System roles are seeded at startup and are protected from deletion.
    /// </summary>
    public bool IsSystemRole { get; init; }

    /// <summary>Gets the permission canonical names granted by this role.</summary>
    public IReadOnlyList<string> Permissions { get; init; } = [];
}
