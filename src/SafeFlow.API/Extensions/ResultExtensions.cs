using Microsoft.AspNetCore.Mvc;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.API.Extensions;

/// <summary>
/// Extension methods on <see cref="Result"/> and <see cref="Result{TValue}"/>
/// that produce the correct <see cref="IActionResult"/> for every error type.
/// </summary>
/// <remarks>
/// All problem responses conform to RFC 7807 via <see cref="ProblemDetails"/>.
/// Stack traces and internal exception messages are never exposed.
/// </remarks>
public static class ResultExtensions
{
    private const string ProblemTypeBase = "https://tools.ietf.org/html/rfc7807";

    /// <summary>
    /// Converts a non-generic <see cref="Result"/> to an <see cref="IActionResult"/>.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="controller">The controller owning this response (for helper access).</param>
    /// <param name="successStatus">HTTP status code on success (default 204 No Content).</param>
    public static IActionResult ToActionResult(
        this Result result,
        ControllerBase controller,
        int successStatus = StatusCodes.Status204NoContent)
    {
        if (result.IsSuccess)
        {
            return successStatus switch
            {
                StatusCodes.Status200OK        => controller.Ok(),
                StatusCodes.Status201Created   => controller.StatusCode(201),
                _                              => controller.NoContent(),
            };
        }

        return MapError(result.Error, controller);
    }

    /// <summary>
    /// Converts a <see cref="Result{TValue}"/> to an <see cref="IActionResult"/>.
    /// </summary>
    /// <typeparam name="TValue">The result value type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="controller">The controller owning this response.</param>
    /// <param name="successStatus">HTTP status code on success (default 200 OK).</param>
    public static IActionResult ToActionResult<TValue>(
        this Result<TValue> result,
        ControllerBase controller,
        int successStatus = StatusCodes.Status200OK)
    {
        if (result.IsSuccess)
        {
            return successStatus switch
            {
                StatusCodes.Status201Created => controller.StatusCode(201, result.Value),
                _                            => controller.Ok(result.Value),
            };
        }

        return MapError(result.Error, controller);
    }

    // ── Private: Error → ProblemDetails mapping ───────────────────────────────

    private static IActionResult MapError(Error error, ControllerBase controller)
    {
        return error.Type switch
        {
            ErrorType.Validation   => controller.UnprocessableEntity(ToProblem(
                StatusCodes.Status422UnprocessableEntity,
                "Validation Error",
                error.Code,
                error.Message,
                controller)),

            ErrorType.NotFound     => controller.NotFound(ToProblem(
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                error.Code,
                error.Message,
                controller)),

            ErrorType.Conflict     => controller.Conflict(ToProblem(
                StatusCodes.Status409Conflict,
                "Conflict",
                error.Code,
                error.Message,
                controller)),

            ErrorType.Unauthorized => controller.Unauthorized(ToProblem(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                error.Code,
                error.Message,
                controller)),

            ErrorType.Forbidden    => controller.StatusCode(
                StatusCodes.Status403Forbidden,
                ToProblem(StatusCodes.Status403Forbidden,
                    "Forbidden", error.Code, error.Message, controller)),

            ErrorType.Business     => controller.BadRequest(ToProblem(
                StatusCodes.Status400BadRequest,
                "Business Rule Violation",
                error.Code,
                error.Message,
                controller)),

            _                      => controller.StatusCode(
                StatusCodes.Status500InternalServerError,
                ToProblem(StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "Server.Error",
                    "An unexpected error occurred. Please try again later.",
                    controller)),
        };
    }

    private static ProblemDetails ToProblem(
        int status,
        string title,
        string errorCode,
        string detail,
        ControllerBase controller)
    {
        return new ProblemDetails
        {
            Status   = status,
            Title    = title,
            Detail   = detail,
            Type     = $"{ProblemTypeBase}#section-{status}",
            Instance = controller.HttpContext.Request.Path,
            Extensions = { ["errorCode"] = errorCode },
        };
    }
}
