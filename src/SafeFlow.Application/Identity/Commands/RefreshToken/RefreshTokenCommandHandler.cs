using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Application.Identity.Specifications;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.Domain.Identity.Entities;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.RefreshToken;

/// <summary>
/// Handles <see cref="RefreshTokenCommand"/> — implements Token Family Rotation
/// with Stolen Token Detection per the approved architecture (ADR-004).
/// </summary>
/// <remarks>
/// <para>Flow:</para>
/// <list type="number">
///   <item>Hash the incoming raw token (SHA-256).</item>
///   <item>Look up the <c>RefreshToken</c> entity by hash.</item>
///   <item>If not found: return 401.</item>
///   <item>
///     If REVOKED: stolen-token detected — revoke the entire family and return 401.
///   </item>
///   <item>If EXPIRED: return 401.</item>
///   <item>Revoke the old token and issue a new one in the same family.</item>
///   <item>Issue a new RS256 access token.</item>
///   <item>Audit and commit.</item>
/// </list>
/// </remarks>
public sealed class RefreshTokenCommandHandler(
    IReadRepository<User> userRepository,
    IRepository<Domain.Identity.Entities.RefreshToken> refreshTokenRepository,
    IJwtTokenService jwtTokenService,
    IAuditService auditService,
    IUnitOfWork unitOfWork,
    ILogger<RefreshTokenCommandHandler> logger)
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponseDto>>
{
    /// <inheritdoc/>
    public async Task<Result<LoginResponseDto>> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        string hash = jwtTokenService.HashToken(command.RefreshTokenValue);

        // ── 1. Look up the stored token by its hash ──────────────────────────
        var tokenSpec = new RefreshTokenByHashSpecification(hash);
        var storedToken = await refreshTokenRepository.FirstOrDefaultAsync(tokenSpec, cancellationToken);

        if (storedToken is null)
        {
            logger.LogWarning("Refresh token not found for hash from IP {IpAddress}", command.IpAddress);

            return Result.Failure<LoginResponseDto>(IdentityErrors.RefreshToken.NotFound);
        }

        // ── 2. Stolen token detection ────────────────────────────────────────
        if (storedToken.IsRevoked)
        {
            logger.LogWarning(
                "Revoked token presented for family {FamilyId} from IP {IpAddress} — revoking entire family",
                storedToken.FamilyId, command.IpAddress);

            // Revoke all active tokens in the same family
            var familySpec = new RefreshTokensByFamilySpecification(storedToken.FamilyId);
            var familyTokens = await refreshTokenRepository.ListAsync(familySpec, cancellationToken);

            foreach (var ft in familyTokens)
            {
                ft.Revoke(command.IpAddress, replacedByTokenHash: null);
                refreshTokenRepository.Update(ft);
            }

            await auditService.LogAsync(
                AuditAction.StolenTokenDetected,
                isSuccess: false,
                ipAddress: command.IpAddress,
                userId: storedToken.UserId,
                failureReason: "Revoked token was presented",
                userAgent: command.UserAgent,
                cancellationToken: cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<LoginResponseDto>(IdentityErrors.RefreshToken.StolenTokenDetected);
        }

        // ── 3. Expiry check ──────────────────────────────────────────────────
        if (storedToken.IsExpired)
        {
            return Result.Failure<LoginResponseDto>(IdentityErrors.RefreshToken.Expired);
        }

        // ── 4. Load user ─────────────────────────────────────────────────────
        var userSpec = new UserByIdWithRolesSpecification(storedToken.UserId);
        var user = await userRepository.FirstOrDefaultAsync(userSpec, cancellationToken);

        if (user is null)
        {
            return Result.Failure<LoginResponseDto>(IdentityErrors.Login.UserNotFound);
        }

        // ── 5. Rotate token ──────────────────────────────────────────────────
        string newRawToken = jwtTokenService.GenerateRefreshToken();
        string newHash = jwtTokenService.HashToken(newRawToken);

        storedToken.Revoke(command.IpAddress, replacedByTokenHash: newHash);
        refreshTokenRepository.Update(storedToken);

        var newRefreshToken = Domain.Identity.Entities.RefreshToken.Create(
            id: Guid.NewGuid(),
            userId: user.Id,
            tokenHash: newHash,
            familyId: storedToken.FamilyId, // Same family — maintains chain
            expiresAt: DateTime.UtcNow.AddDays(jwtTokenService.RefreshTokenExpirationDays),
            createdByIp: command.IpAddress);

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        // ── 6. Issue new access token ────────────────────────────────────────
        var roles = user.UserRoles.Select(ur => ur.RoleId.ToString()).ToList();

        string accessToken = jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email.Value,
            user.FullName.ToString(),
            tenantId: Guid.Empty,
            roles: roles,
            permissions: []);

        await auditService.LogAsync(
            AuditAction.TokenRefreshed,
            isSuccess: true,
            ipAddress: command.IpAddress,
            userId: user.Id,
            userAgent: command.UserAgent,
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Refresh token rotated for user {UserId}", user.Id);

        return Result.Success(new LoginResponseDto
        {
            AccessToken = accessToken,
            ExpiresIn = jwtTokenService.AccessTokenExpirationMinutes * 60,
            RefreshToken = newRawToken,
            User = new UserSummaryDto
            {
                Id = user.Id,
                Email = user.Email.Value,
                FullName = user.FullName.ToString(),
                Roles = roles.AsReadOnly(),
            },
        });
    }
}
