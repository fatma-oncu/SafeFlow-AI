using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Application.Identity.Specifications;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.ForgotPassword;

/// <summary>
/// Handles <see cref="ForgotPasswordCommand"/>.
/// </summary>
/// <remarks>
/// Intentionally returns <see cref="Result.Success()"/> unconditionally to
/// prevent email-enumeration attacks. If the address is registered, a reset
/// token is generated and dispatched via <see cref="IEmailService"/>.
/// </remarks>
public sealed class ForgotPasswordCommandHandler(
    IReadRepository<User> userRepository,
    IIdentityService identityService,
    IEmailService emailService,
    ILogger<ForgotPasswordCommandHandler> logger)
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    /// <inheritdoc/>
    public async Task<Result> Handle(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        // ── 1. Look up user — deliberately silent on miss ─────────────────────
        var spec = new UserByEmailSpecification(command.Email);
        var user = await userRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (user is null)
        {
            // OWASP: return success regardless — do NOT indicate the email is unknown
            logger.LogInformation(
                "ForgotPassword requested for unknown email (suppressed). IP={IP}",
                command.IpAddress);

            return Result.Success();
        }

        // ── 2. Generate password reset token ──────────────────────────────────
        var tokenResult = await identityService.GeneratePasswordResetTokenAsync(
            user.Id, cancellationToken);

        if (tokenResult.IsFailure)
        {
            // Log the internal failure but still return success to caller
            logger.LogWarning(
                "Failed to generate reset token for UserId={UserId}: {Error}",
                user.Id, tokenResult.Error.Message);

            return Result.Success();
        }

        // ── 3. Send email ─────────────────────────────────────────────────────
        await emailService.SendEmailAsync(
            toAddress: command.Email,
            subject: "Şifre Sıfırlama Talebi",
            htmlBody: $"""
                <p>Şifrenizi sıfırlamak için aşağıdaki token'ı kullanınız:</p>
                <p><strong>{tokenResult.Value}</strong></p>
                <p>Bu talep sizden gelmiyorsa lütfen dikkate almayınız.</p>
                """,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Password reset email dispatched for UserId={UserId}. IP={IP}",
            user.Id, command.IpAddress);

        return Result.Success();
    }
}
