using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.ResetPassword;

/// <summary>
/// Resets a user's password using a valid password reset token.
/// </summary>
/// <remarks>
/// The reset token is issued by the identity provider and is validated by the
/// application service. After a successful password reset, all active refresh
/// tokens belonging to the user should be revoked to invalidate existing sessions.
/// </remarks>
/// <param name="UserId">
/// The unique identifier of the user whose password will be reset.
/// </param>
/// <param name="Token">
/// The password reset token previously issued to the user.
/// </param>
/// <param name="NewPassword">
/// The new plaintext password that satisfies the application's password policy.
/// </param>
/// <param name="IpAddress">
/// The client IP address captured for security auditing.
/// </param>
/// <param name="UserAgent">
/// The client User-Agent header captured for security auditing. May be <see langword="null"/>.
/// </param>
public sealed record ResetPasswordCommand(
    Guid UserId,
    string Token,
    string NewPassword,
    string IpAddress,
    string? UserAgent) : IRequest<Result>;