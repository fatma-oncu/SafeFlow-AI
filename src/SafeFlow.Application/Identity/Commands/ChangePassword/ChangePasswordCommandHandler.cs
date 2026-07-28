using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Application.Identity.Specifications;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;
using RefreshTokenEntity = SafeFlow.Domain.Identity.Entities.RefreshToken;

namespace SafeFlow.Application.Identity.Commands.ChangePassword;

/// <summary>
/// Handles <see cref="ChangePasswordCommand"/>.
/// </summary>
/// <remarks>
/// <para>
/// The caller's identity is resolved from <see cref="ICurrentUserService"/>.
/// The UserId is never taken from the command itself to prevent privilege escalation.
/// </para>
/// <para>
/// After a successful password change, all active refresh tokens are revoked to
/// force re-authentication on all devices (per OWASP Session Management Cheat Sheet).
/// </para>
/// </remarks>
public sealed class ChangePasswordCommandHandler(
    ICurrentUserService currentUserService,
    IIdentityService identityService,
    IRepository<RefreshTokenEntity> refreshTokenRepository,
    IAuditService auditService,
    IUnitOfWork unitOfWork,
    ILogger<ChangePasswordCommandHandler> logger)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
        {
            return Result.Failure(IdentityErrors.User.NotAuthenticated);
        }

        Guid userId = currentUserService.UserId.Value;

        // ── 1. Change password via Identity ──────────────────────────────────
        var changeResult = await identityService.ChangePasswordAsync(
            userId,
            command.CurrentPassword,
            command.NewPassword,
            cancellationToken);

        if (changeResult.IsFailure)
        {
            await auditService.LogAsync(
                AuditAction.PasswordChanged,
                isSuccess: false,
                ipAddress: command.IpAddress,
                userId: userId,
                failureReason: changeResult.Error.Message,
                userAgent: command.UserAgent,
                cancellationToken: cancellationToken);

            return changeResult;
        }

        // ── 2. Revoke all active refresh tokens ───────────────────────────────
        var activeTokensSpec = new ActiveRefreshTokensByUserSpecification(userId);
        var activeTokens = await refreshTokenRepository.ListAsync(activeTokensSpec, cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(command.IpAddress, replacedByTokenHash: null);
            refreshTokenRepository.Update(token);
        }

        await auditService.LogAsync(
            AuditAction.PasswordChanged,
            isSuccess: true,
            ipAddress: command.IpAddress,
            userId: userId,
            userAgent: command.UserAgent,
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Password changed for user {UserId}. {TokenCount} active tokens revoked.",
            userId, activeTokens.Count);

        return Result.Success();
    }
}
