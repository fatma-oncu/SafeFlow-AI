using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeFlow.API.Extensions;
using SafeFlow.API.Models.Auth;
using SafeFlow.Application.Identity.Commands.ChangePassword;
using SafeFlow.Application.Identity.Commands.ForgotPassword;
using SafeFlow.Application.Identity.Commands.Login;
using SafeFlow.Application.Identity.Commands.Logout;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Application.Identity.Commands.RegisterUser;
using SafeFlow.Application.Identity.Commands.ResetPassword;
using RefreshTokenCommand = SafeFlow.Application.Identity.Commands.RefreshToken.RefreshTokenCommand;

namespace SafeFlow.API.Controllers;

/// <summary>
/// Identity and session management endpoints.
/// </summary>
/// <remarks>
/// Authentication endpoints (login, register, refresh, forgot-password,
/// reset-password) are publicly accessible.  All other endpoints require
/// a valid JWT Bearer token.
/// </remarks>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json")]
public sealed class AuthController : ApiControllerBase
{
    // ── POST /api/v1/auth/register ────────────────────────────────────────────

    /// <summary>Registers a new user account.</summary>
    /// <param name="request">Registration details.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>The new user's identifier on success (201 Created).</returns>
    /// <response code="201">User created successfully.</response>
    /// <response code="409">Email is already registered.</response>
    /// <response code="422">Validation errors in the request body.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            Email:       request.Email,
            Password:    request.Password,
            FirstName:   request.FirstName,
            LastName:    request.LastName,
            PhoneNumber: request.PhoneNumber,
            TenantId:    request.TenantId,
            IpAddress:   GetClientIp(),
            UserAgent:   GetUserAgent());

        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this, StatusCodes.Status201Created);
    }

    // ── POST /api/v1/auth/login ───────────────────────────────────────────────

    /// <summary>Authenticates a user and issues JWT access + refresh tokens.</summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>Access token, expiry, and user summary on success.</returns>
    /// <response code="200">Authentication successful.</response>
    /// <response code="400">Account is locked or inactive.</response>
    /// <response code="401">Invalid credentials.</response>
    /// <response code="422">Validation errors.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(
            Email:     request.Email,
            Password:  request.Password,
            IpAddress: GetClientIp(),
            UserAgent: GetUserAgent());

        var result = await Mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            // Write refresh token to HttpOnly cookie for browser clients
            SetRefreshTokenCookie(result.Value.RefreshToken);

            // Return access token only in body (never raw refresh token for browser)
            var responseBody = result.Value with { RefreshToken = null };
            return Ok(responseBody);
        }

        return result.ToActionResult(this);
    }

    // ── POST /api/v1/auth/refresh ─────────────────────────────────────────────

    /// <summary>Rotates a refresh token and issues a new access token.</summary>
    /// <param name="request">The refresh token (used when not reading from cookie).</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>New access token and refresh token on success.</returns>
    /// <response code="200">Token rotation successful.</response>
    /// <response code="401">Refresh token is invalid, expired, or revoked.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest? request,
        CancellationToken cancellationToken)
    {
        // Prefer the HttpOnly cookie; fall back to the request body (mobile clients)
        string? tokenValue = HttpContext.Request.Cookies[CookieNames.RefreshToken]
                             ?? request?.RefreshToken;

        if (string.IsNullOrWhiteSpace(tokenValue))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = 401,
                Title  = "Unauthorized",
                Detail = "Refresh token is required.",
            });
        }

        var command = new RefreshTokenCommand(
            RefreshTokenValue: tokenValue,
            IpAddress:         GetClientIp(),
            UserAgent:         GetUserAgent());

        var result = await Mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            SetRefreshTokenCookie(result.Value.RefreshToken);
            var responseBody = result.Value with { RefreshToken = null };
            return Ok(responseBody);
        }

        return result.ToActionResult(this);
    }

    // ── POST /api/v1/auth/logout ──────────────────────────────────────────────

    /// <summary>Revokes the current session's refresh token.</summary>
    /// <param name="request">The refresh token (used when not reading from cookie).</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <response code="204">Logout successful.</response>
    /// <response code="401">Refresh token not provided or invalid.</response>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest? request,
        CancellationToken cancellationToken)
    {
        string? tokenValue = HttpContext.Request.Cookies[CookieNames.RefreshToken]
                             ?? request?.RefreshToken;

        if (string.IsNullOrWhiteSpace(tokenValue))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = 401,
                Title  = "Unauthorized",
                Detail = "Refresh token is required.",
            });
        }

        var command = new LogoutCommand(
            RefreshTokenValue: tokenValue,
            IpAddress:         GetClientIp(),
            UserAgent:         GetUserAgent());

        var result = await Mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            DeleteRefreshTokenCookie();
        }

        return result.ToActionResult(this);
    }

    // ── POST /api/v1/auth/change-password ─────────────────────────────────────

    /// <summary>Changes the authenticated user's password.</summary>
    /// <param name="request">Current and new password.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <response code="204">Password changed successfully.</response>
    /// <response code="400">Current password is incorrect or new password violates policy.</response>
    /// <response code="401">Not authenticated.</response>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(
            CurrentPassword: request.CurrentPassword,
            NewPassword:     request.NewPassword,
            IpAddress:       GetClientIp(),
            UserAgent:       GetUserAgent());

        var result = await Mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            DeleteRefreshTokenCookie(); // Force re-login after password change
        }

        return result.ToActionResult(this);
    }

    // ── POST /api/v1/auth/forgot-password ─────────────────────────────────────

    /// <summary>Initiates a password-reset flow by dispatching a reset email.</summary>
    /// <param name="request">The email address of the user requesting the reset.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <response code="202">Request accepted (always, to prevent email enumeration).</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(
            Email:     request.Email,
            IpAddress: GetClientIp(),
            UserAgent: GetUserAgent());

        // Always dispatch; always return 202 — caller cannot enumerate emails
        await Mediator.Send(command, cancellationToken);
        return Accepted();
    }

    // ── POST /api/v1/auth/reset-password ──────────────────────────────────────

    /// <summary>Resets a user's password using a valid password-reset token.</summary>
    /// <param name="request">User identifier, reset token, and new password.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <response code="204">Password reset successfully.</response>
    /// <response code="400">Token is invalid or expired, or new password violates policy.</response>
    /// <response code="422">Validation errors.</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(
            UserId:      request.UserId,
            Token:       request.Token,
            NewPassword: request.NewPassword,
            IpAddress:   GetClientIp(),
            UserAgent:   GetUserAgent());

        var result = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    // ── Cookie helpers ────────────────────────────────────────────────────────

    private void SetRefreshTokenCookie(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return;

        HttpContext.Response.Cookies.Append(
            CookieNames.RefreshToken,
            token,
            new CookieOptions
            {
                HttpOnly  = true,
                Secure    = true,
                SameSite  = SameSiteMode.Strict,
                Expires   = DateTimeOffset.UtcNow.AddDays(7),
                Path      = "/api/v1/auth",
            });
    }

    private void DeleteRefreshTokenCookie()
    {
        HttpContext.Response.Cookies.Delete(
            CookieNames.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure   = true,
                SameSite = SameSiteMode.Strict,
                Path     = "/api/v1/auth",
            });
    }
}

/// <summary>Cookie name constants used by <see cref="AuthController"/>.</summary>
internal static class CookieNames
{
    /// <summary>Name of the HttpOnly refresh-token cookie.</summary>
    internal const string RefreshToken = "sf_rt";
}
