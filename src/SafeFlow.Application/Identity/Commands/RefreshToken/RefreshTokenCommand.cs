using MediatR;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.RefreshToken;

/// <summary>
/// Rotates a refresh token: validates the incoming token, revokes it, and issues a
/// new access token and a new refresh token under the same token family.
/// </summary>
/// <param name="RefreshTokenValue">
/// The raw refresh token value received from the client (cookie or header).
/// </param>
/// <param name="IpAddress">The originating client IP address.</param>
/// <param name="UserAgent">The HTTP User-Agent header, for audit logging.</param>
public sealed record RefreshTokenCommand(
    string RefreshTokenValue,
    string IpAddress,
    string? UserAgent) : IRequest<Result<LoginResponseDto>>;
