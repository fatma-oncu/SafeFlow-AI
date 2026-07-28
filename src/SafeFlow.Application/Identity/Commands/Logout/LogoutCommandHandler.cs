using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Application.Identity.Specifications;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;
using RefreshTokenEntity = SafeFlow.Domain.Identity.Entities.RefreshToken;

namespace SafeFlow.Application.Identity.Commands.Logout;

/// <summary>
/// Handles <see cref="LogoutCommand"/> by revoking the presented refresh token.
/// </summary>
public sealed class LogoutCommandHandler(
    IRepository<RefreshTokenEntity> refreshTokenRepository,
    IJwtTokenService jwtTokenService,
    IAuditService auditService,
    IUnitOfWork unitOfWork,
    ILogger<LogoutCommandHandler> logger)
    : IRequestHandler<LogoutCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        string hash = jwtTokenService.HashToken(command.RefreshTokenValue);
        var spec = new RefreshTokenByHashSpecification(hash);
        var token = await refreshTokenRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (token is null || token.IsRevoked)
        {
            // Treat missing / already-revoked tokens as success — idempotent logout.
            logger.LogInformation("Logout: token not found or already revoked from IP {IpAddress}", command.IpAddress);
            return Result.Success();
        }

        token.Revoke(command.IpAddress);
        refreshTokenRepository.Update(token);

        await auditService.LogAsync(
            AuditAction.Logout,
            isSuccess: true,
            ipAddress: command.IpAddress,
            userId: token.UserId,
            userAgent: command.UserAgent,
            cancellationToken: cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("User {UserId} logged out from IP {IpAddress}", token.UserId, command.IpAddress);

        return Result.Success();
    }
}
