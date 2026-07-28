using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace SafeFlow.API.Controllers;

/// <summary>
/// Base class for all SafeFlow API controllers.
/// </summary>
/// <remarks>
/// Provides access to <see cref="Mediator"/> and common HTTP helper utilities
/// (IP resolution, User-Agent extraction). All controllers are thin — they only
/// perform model binding, send commands/queries via MediatR, and map results to HTTP.
/// </remarks>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// Gets the MediatR <see cref="ISender"/> resolved from the current request scope.
    /// </summary>
    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Extracts the client IP address from the request.
    /// Prefers <c>X-Forwarded-For</c> (set by reverse proxy) over the remote address.
    /// </summary>
    protected string GetClientIp()
    {
        var forwarded = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            // X-Forwarded-For may be a comma-separated list; take the first value
            return forwarded.Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Returns the <c>User-Agent</c> header value, or <c>null</c> when absent.
    /// </summary>
    protected string? GetUserAgent() =>
        HttpContext.Request.Headers.UserAgent.FirstOrDefault();
}
