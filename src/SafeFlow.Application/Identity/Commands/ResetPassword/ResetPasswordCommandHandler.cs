using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Application.Identity.Specifications;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;
using RefreshTokenEntity = SafeFlow.Domain.Identity.Entities.RefreshToken;

namespace SafeFlow.Application.Identity.Commands.ResetPassword;

/// <summary>
/// Handles <see cref="ResetPasswordCommand"/>.
/// </summary>
/// <remarks>
/// All active refresh tokens are revoked after a successful reset to force
/// re-authentication on all devices.
/// </remarks>
public sealed class ResetPasswordCommandHandler(
    IIdentityService identityService,
    IRepository<RefreshTokenEntity> refreshTokenRepository,
    IAuditService auditService,
    IUnitOfWork unitOfWork,
    ILogger<ResetPasswordCommandHandler> logger)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var resetResult = await identityService.ResetPasswordAsync(
            command.UserId,
            command.Token,
            command.NewPassword,
            cancellationToken);

        if (resetResult.IsFailure)
        {
            await auditService.LogAsync(
                AuditAction.PasswordReset,
                isSuccess: false,
                ipAddress: command.IpAddress,
                userId: command.UserId,
                failureReason: resetResult.Error.Message,
                userAgent: command.UserAgent,
                cancellationToken: cancellationToken);

            return resetResult;
        }

        // Revoke all active sessions after password reset
        var activeTokensSpec = new ActiveRefreshTokensByUserSpecification(command.UserId);
        var activeTokens = await refreshTokenRepository.ListAsync(activeTokensSpec, cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(command.IpAddress, replacedByTokenHash: null);
            refreshTokenRepository.Update(token);
        }

        await auditService.LogAsync(
            AuditAction.PasswordReset,
            isSuccess: true,
            ipAddress: command.IpAddress,
            userId: command.UserId,
            userAgent: command.UserAgent,
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Password reset for user {UserId}. {TokenCount} sessions revoked.",
            command.UserId, activeTokens.Count);

        return Result.Success();
    }
}
