using Serilog.Context;

namespace SafeFlow.API.Middleware;

/// <summary>
/// Middleware that ensures every request carries a correlation identifier.
/// </summary>
/// <remarks>
/// <para>
/// If the incoming request contains an <c>X-Correlation-ID</c> header the value is
/// reused; otherwise a new <see cref="Guid"/> is generated. The value is:
/// <list type="bullet">
///   <item>pushed into <see cref="LogContext"/> so every Serilog event emitted
///         during the request is automatically enriched with <c>CorrelationId</c>;</item>
///   <item>echoed back on the response in the same header so callers can
///         correlate client-side and server-side traces.</item>
/// </list>
/// </para>
/// <para>
/// This middleware must be registered <em>before</em>
/// <see cref="GlobalExceptionMiddleware"/> so that all downstream log events —
/// including exception events — already carry the correlation identifier.
/// </para>
/// </remarks>
public sealed class CorrelationIdMiddleware
{
    /// <summary>The HTTP header name used to propagate the correlation identifier.</summary>
    public const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;

    /// <summary>Initialises a new <see cref="CorrelationIdMiddleware"/>.</summary>
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = ResolveCorrelationId(context.Request);

        // Echo the ID back on the response before any downstream middleware runs
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // Push into LogContext — disposable scope automatically pops when request ends
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string ResolveCorrelationId(HttpRequest request)
    {
        if (request.Headers.TryGetValue(HeaderName, out var existing)
            && !string.IsNullOrWhiteSpace(existing))
        {
            return existing.ToString();
        }

        return Guid.NewGuid().ToString("D");
    }
}
