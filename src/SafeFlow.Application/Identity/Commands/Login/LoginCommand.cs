using MediatR;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.Login;

/// <summary>
/// Authenticates a user with email and password, issuing a JWT access token and a
/// refresh token.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The raw (unhashed) password.</param>
/// <param name="IpAddress">The originating client IP address, used for token storage and audit.</param>
/// <param name="UserAgent">The HTTP User-Agent header, for audit logging.</param>
public sealed record LoginCommand(
    string Email,
    string Password,
    string IpAddress,
    string? UserAgent) : IRequest<Result<LoginResponseDto>>;
