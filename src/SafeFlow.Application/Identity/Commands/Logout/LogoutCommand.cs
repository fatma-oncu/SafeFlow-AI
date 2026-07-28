using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.Logout;

/// <summary>
/// Revokes the given refresh token, effectively logging the user out of the current session.
/// The API layer is responsible for clearing the <c>HttpOnly</c> cookie after this command succeeds.
/// </summary>
/// <param name="RefreshTokenValue">
/// The raw refresh token value received from the client (cookie or header).
/// </param>
/// <param name="IpAddress">The originating client IP address.</param>
/// <param name="UserAgent">The HTTP User-Agent header, for audit logging.</param>
public sealed record LogoutCommand(
    string RefreshTokenValue,
    string IpAddress,
    string? UserAgent) : IRequest<Result>;
