using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.ChangePassword;

/// <summary>
/// Changes the authenticated user's password after verifying the current one.
/// All active refresh tokens are revoked after a successful password change
/// to force re-authentication on all devices.
/// </summary>
/// <param name="CurrentPassword">The user's existing raw password for verification.</param>
/// <param name="NewPassword">The new raw password to set.</param>
/// <param name="IpAddress">The originating client IP address, for audit logging.</param>
/// <param name="UserAgent">The HTTP User-Agent header, for audit logging.</param>
public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string IpAddress,
    string? UserAgent) : IRequest<Result>;
