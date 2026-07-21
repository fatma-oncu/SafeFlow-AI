using SafeFlow.SharedKernel.Entities;
using SafeFlow.SharedKernel.Exceptions;

namespace SafeFlow.Domain.Identity.Entities;

/// <summary>
/// Represents a refresh token issued during authentication, stored as a secure
/// hash rather than a raw token value.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RefreshToken"/> implements a token-family rotation strategy:
/// each token belongs to a <see cref="FamilyId"/> that groups a chain of
/// successively rotated tokens.  If a revoked token from a family is presented,
/// the application layer must revoke all active tokens in that family to mitigate
/// token-theft attacks.
/// </para>
/// <para>
/// Only the HMAC-SHA256 hash of the raw token is persisted — the raw token is
/// returned to the client once and never stored, following OWASP secure token
/// storage guidelines.
/// </para>
/// <para>
/// The IP address fields (<see cref="CreatedByIp"/>, <see cref="RevokedByIp"/>)
/// are advisory audit data and must not be used for access-control decisions.
/// </para>
/// </remarks>
public sealed class RefreshToken : BaseEntity
{
    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Private parameterless constructor reserved for ORM materialisation.
    /// </summary>
    private RefreshToken() { }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the identifier of the <c>User</c> this token belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the cryptographic hash of the raw refresh token value.
    /// The hash algorithm is determined by the Infrastructure layer (HMAC-SHA256 recommended).
    /// </summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the family identifier that groups this token with its rotation predecessors
    /// and successors.  Used to detect and respond to token-reuse attacks.
    /// </summary>
    public Guid FamilyId { get; private set; }

    /// <summary>
    /// Gets the UTC instant at which this refresh token expires and can no longer
    /// be used to obtain a new access token.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Gets the UTC instant at which this token was explicitly revoked, or
    /// <c>null</c> if it has not been revoked.
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// Gets the hash of the replacement token when this token was rotated, or
    /// <c>null</c> if the token was not replaced (e.g., it was revoked due to
    /// a reuse attack or an explicit sign-out).
    /// </summary>
    public string? ReplacedByTokenHash { get; private set; }

    /// <summary>
    /// Gets the IP address of the client that created this token, or <c>null</c>
    /// if the IP was not recorded (e.g., background jobs or internal services).
    /// </summary>
    public string? CreatedByIp { get; private set; }

    /// <summary>
    /// Gets the IP address of the client that triggered revocation of this token,
    /// or <c>null</c> if not yet revoked.
    /// </summary>
    public string? RevokedByIp { get; private set; }

    // -------------------------------------------------------------------------
    // Computed properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether this token has passed its expiry time.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Gets a value indicating whether this token has been explicitly revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    /// <summary>
    /// Gets a value indicating whether this token is currently usable:
    /// not revoked and not expired.
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new active <see cref="RefreshToken"/> entity.
    /// </summary>
    /// <param name="id">The unique identifier for this token entity.</param>
    /// <param name="userId">The identifier of the owning user.</param>
    /// <param name="tokenHash">
    /// The cryptographic hash of the raw token value.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="familyId">
    /// The family identifier for token-rotation tracking.
    /// </param>
    /// <param name="expiresAt">
    /// The UTC expiry time. Must be in the future at the time of creation.
    /// </param>
    /// <param name="createdByIp">
    /// The IP address of the client requesting the token. Optional.
    /// </param>
    /// <returns>A new, active <see cref="RefreshToken"/> entity.</returns>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="tokenHash"/> is <c>null</c> or whitespace, or
    /// when <paramref name="expiresAt"/> is not in the future.
    /// </exception>
    public static RefreshToken Create(
        Guid id,
        Guid userId,
        string tokenHash,
        Guid familyId,
        DateTime expiresAt,
        string? createdByIp = null)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ValidationException(nameof(tokenHash), "Token hash must not be empty.");

        if (expiresAt <= DateTime.UtcNow)
            throw new ValidationException(nameof(expiresAt), "Token expiry must be in the future.");

        var entity = new RefreshToken();
        entity.Id = id;
        entity.UserId = userId;
        entity.TokenHash = tokenHash;
        entity.FamilyId = familyId;
        entity.ExpiresAt = expiresAt;
        entity.CreatedByIp = createdByIp;
        return entity;
    }

    // -------------------------------------------------------------------------
    // Domain methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Revokes this token, preventing it from being used again.
    /// </summary>
    /// <param name="revokedByIp">
    /// The IP address of the client that triggered revocation. Optional.
    /// </param>
    /// <param name="replacedByTokenHash">
    /// The hash of the token that replaced this one during rotation, or
    /// <c>null</c> when revocation is not part of a rotation (e.g., explicit sign-out).
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the token is already revoked.
    /// </exception>
    public void Revoke(string? revokedByIp = null, string? replacedByTokenHash = null)
    {
        if (IsRevoked)
            throw new InvalidOperationException(
                $"Refresh token '{Id}' has already been revoked.");

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
