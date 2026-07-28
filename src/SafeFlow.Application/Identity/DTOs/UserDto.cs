namespace SafeFlow.Application.Identity.DTOs;

/// <summary>
/// A read-only projection of a <c>User</c> aggregate suitable for external consumption.
/// </summary>
/// <remarks>
/// <para>
/// Sensitive fields (password hash, refresh token hashes, failed-login counters) are
/// intentionally absent. This DTO is safe to serialize and return from API endpoints.
/// </para>
/// </remarks>
public sealed record UserDto
{
    /// <summary>Gets the user's unique identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the user's normalised email address.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Gets the user's first name.</summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>Gets the user's last name.</summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>Gets the user's full display name (<c>FirstName LastName</c>).</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Gets the user's phone number, or <c>null</c> when not provided.</summary>
    public string? PhoneNumber { get; init; }

    /// <summary>Gets a value indicating whether the account is active.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets a value indicating whether the account is currently locked.</summary>
    public bool IsLocked { get; init; }

    /// <summary>
    /// Gets the UTC timestamp of the most recent successful login, or <c>null</c> if
    /// the user has never logged in.
    /// </summary>
    public DateTime? LastLoginAt { get; init; }

    /// <summary>Gets the role names currently assigned to this user.</summary>
    public IReadOnlyList<string> Roles { get; init; } = [];

    /// <summary>Gets the permission canonical names granted through the user's roles.</summary>
    public IReadOnlyList<string> Permissions { get; init; } = [];
}
