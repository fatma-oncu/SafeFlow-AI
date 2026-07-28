namespace SafeFlow.Application.Identity.Interfaces;

/// <summary>
/// Defines the contract for RS256-based JWT access-token and refresh-token operations.
/// Implemented exclusively in the Infrastructure layer.
/// </summary>
/// <remarks>
/// <para>
/// Access tokens are signed with RS256 (asymmetric RSA-SHA256). The private key never
/// leaves the server; the public key may be distributed to downstream services.
/// </para>
/// <para>
/// Refresh tokens are cryptographically random 64-byte values encoded as Base64.
/// Only the SHA-256 hash of the raw refresh token is persisted in the database —
/// the raw value is returned to the client once and never stored.
/// </para>
/// </remarks>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a short-lived RS256-signed JWT access token carrying the given claims.
    /// </summary>
    /// <param name="userId">The subject identifier to embed in the <c>sub</c> claim.</param>
    /// <param name="email">The user's email for the <c>email</c> claim.</param>
    /// <param name="fullName">The user's display name for the <c>name</c> claim.</param>
    /// <param name="tenantId">The tenant identifier embedded as a custom claim.</param>
    /// <param name="roles">Role names to embed as role claims.</param>
    /// <param name="permissions">Permission canonical names to embed as permission claims.</param>
    /// <returns>A signed JWT string.</returns>
    string GenerateAccessToken(
        Guid userId,
        string email,
        string fullName,
        Guid tenantId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions);

    /// <summary>
    /// Generates a cryptographically secure random refresh token (64 bytes, Base64-encoded).
    /// The caller is responsible for hashing this value before persisting it.
    /// </summary>
    /// <returns>The raw, unhashed refresh token string.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Computes the SHA-256 hash of the given token string.
    /// Used to derive the storable hash from the raw refresh token.
    /// </summary>
    /// <param name="token">The raw token value to hash.</param>
    /// <returns>A hex-encoded SHA-256 hash string.</returns>
    string HashToken(string token);

    /// <summary>
    /// Gets the lifetime of a newly issued access token, in minutes.
    /// </summary>
    int AccessTokenExpirationMinutes { get; }

    /// <summary>
    /// Gets the lifetime of a newly issued refresh token, in days.
    /// </summary>
    int RefreshTokenExpirationDays { get; }
}
