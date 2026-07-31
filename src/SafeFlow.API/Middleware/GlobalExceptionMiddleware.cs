using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.API.Middleware;

/// <summary>
/// Terminal middleware that catches all unhandled exceptions and converts them
/// to RFC 7807 <see cref="ProblemDetails"/> responses.
/// </summary>
/// <remarks>
/// <para>
/// Stack traces and internal exception messages are never included in
/// production responses. The <c>Instance</c> field is set to the request path
/// so the caller can correlate responses with logs.
/// </para>
/// <para>
/// This middleware must be registered as the <em>first</em> middleware in the
/// pipeline so that it can catch exceptions thrown by all subsequent middleware,
/// including routing, authentication, and controller execution.
/// </para>
/// </remarks>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    /// <summary>Initialises a new <see cref="GlobalExceptionMiddleware"/>.</summary>
    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    /// <summary>Invokes the middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unhandled exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            await WriteErrorResponseAsync(context, ex);
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var (status, title, code) = ClassifyException(exception);

        // In development or for validation exceptions expose detail message.
        string detail = (_env.IsDevelopment() || exception is SafeFlow.SharedKernel.Exceptions.ValidationException)
            ? exception.Message
            : "An unexpected error occurred. Please try again later.";

        var problem = new ProblemDetails
        {
            Status   = status,
            Title    = title,
            Detail   = detail,
            Type     = $"https://tools.ietf.org/html/rfc7807#section-{status}",
            Instance = context.Request.Path,
            Extensions = { ["errorCode"] = code },
        };

        if (exception is SafeFlow.SharedKernel.Exceptions.ValidationException valEx)
        {
            problem.Extensions["errors"] = valEx.Errors;
        }

        context.Response.StatusCode  = status;
        context.Response.ContentType = "application/problem+json";

        return context.Response.WriteAsync(
            JsonSerializer.Serialize(problem, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
    }

    private static (int Status, string Title, string Code) ClassifyException(Exception ex) =>
        ex switch
        {
            BadHttpRequestException bhr                          => (bhr.StatusCode, "Bad Request", "Request.Invalid"),
            SafeFlow.SharedKernel.Exceptions.ValidationException => (400, "Bad Request", "Validation.Error"),
            ArgumentException                                    => (400, "Bad Request", "Request.Argument"),
            UnauthorizedAccessException                          => (401, "Unauthorized", "Auth.Unauthorized"),
            _                                                    => (500, "Internal Server Error", "Server.Error"),
        };
}
