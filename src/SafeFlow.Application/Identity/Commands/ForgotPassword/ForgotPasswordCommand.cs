using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.ForgotPassword;

/// <summary>
/// Initiates a password-reset flow by generating a reset token and sending a
/// password-reset email to the address on file.
/// </summary>
/// <remarks>
/// <para>
/// To prevent email enumeration (OWASP A07:2021), the handler ALWAYS returns
/// <c>Result.Success()</c> regardless of whether the email is registered.
/// The caller cannot distinguish between an unknown address and a successful
/// token dispatch.
/// </para>
/// </remarks>
/// <param name="Email">The email address of the user requesting the reset.</param>
/// <param name="IpAddress">The client IP address, for audit logging.</param>
/// <param name="UserAgent">The HTTP User-Agent header, for audit logging.</param>
public sealed record ForgotPasswordCommand(
    string Email,
    string IpAddress,
    string? UserAgent) : IRequest<Result>;
