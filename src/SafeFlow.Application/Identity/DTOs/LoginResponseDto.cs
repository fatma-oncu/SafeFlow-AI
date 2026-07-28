namespace SafeFlow.Application.Identity.DTOs;

/// <summary>
/// Represents the token payload returned on a successful login or token refresh.
/// </summary>
/// <remarks>
/// <para>
/// The raw refresh token value is <b>never</b> included in this DTO for browser clients —
/// it is delivered exclusively via an <c>HttpOnly; Secure; SameSite=Strict</c> cookie
/// set by the API layer. Mobile clients (Flutter) receive it in this DTO as an exception
/// to support <c>flutter_secure_storage</c>.
/// </para>
/// <para>
/// The API controller decides whether to write a cookie or populate
/// <see cref="RefreshToken"/>, based on the client type.
/// </para>
/// </remarks>
public sealed record LoginResponseDto
{
    /// <summary>Gets the RS256-signed JWT access token.</summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>Gets the number of seconds until the access token expires.</summary>
    public int ExpiresIn { get; init; }

    /// <summary>
    /// Gets the raw refresh token value.
    /// Set only for mobile clients; <c>null</c> for browser clients (cookie is used).
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>Gets a lightweight summary of the authenticated user.</summary>
    public UserSummaryDto User { get; init; } = default!;
}

/// <summary>
/// A minimal user summary embedded in authentication responses.
/// </summary>
/// <remarks>
/// Contains only the fields required for the client to render a welcome state.
/// Full profile data is available via <c>GET /v1/auth/me</c>.
/// </remarks>
public sealed record UserSummaryDto
{
    /// <summary>Gets the user's unique identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Gets the user's email address.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Gets the user's full display name.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Gets the role names assigned to the user.</summary>
    public IReadOnlyList<string> Roles { get; init; } = [];
}
