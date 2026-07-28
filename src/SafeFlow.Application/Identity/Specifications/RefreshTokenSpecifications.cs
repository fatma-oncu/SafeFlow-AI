using SafeFlow.Domain.Identity.Entities;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.Identity.Specifications;

/// <summary>
/// Retrieves a <see cref="RefreshToken"/> entity by its stored SHA-256 token hash.
/// </summary>
public sealed class RefreshTokenByHashSpecification : BaseSpecification<RefreshToken>
{
    /// <summary>
    /// Initializes the specification with the given <paramref name="tokenHash"/>.
    /// </summary>
    /// <param name="tokenHash">The SHA-256 hash of the raw refresh token presented by the client.</param>
    public RefreshTokenByHashSpecification(string tokenHash)
        : base(rt => rt.TokenHash == tokenHash)
    {
        ApplyNoTracking();
    }
}

/// <summary>
/// Retrieves all <see cref="RefreshToken"/> entities belonging to the given token family.
/// Used during stolen-token detection to revoke the entire family at once.
/// </summary>
public sealed class RefreshTokensByFamilySpecification : BaseSpecification<RefreshToken>
{
    /// <summary>
    /// Initializes the specification with the given <paramref name="familyId"/>.
    /// </summary>
    /// <param name="familyId">The family identifier shared by a chain of rotated tokens.</param>
    public RefreshTokensByFamilySpecification(Guid familyId)
        : base(rt => rt.FamilyId == familyId && rt.RevokedAt == null)
    {
    }
}

/// <summary>
/// Retrieves all active (non-revoked, non-expired) <see cref="RefreshToken"/> entities
/// belonging to the given user. Used during password change/reset to revoke all sessions.
/// </summary>
public sealed class ActiveRefreshTokensByUserSpecification : BaseSpecification<RefreshToken>
{
    /// <summary>
    /// Initializes the specification with the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user whose active tokens should be revoked.</param>
    public ActiveRefreshTokensByUserSpecification(Guid userId)
        : base(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
    {
    }
}
