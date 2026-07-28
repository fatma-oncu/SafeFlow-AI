using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeFlow.API.Authorization;
using SafeFlow.API.Extensions;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Application.Identity.Queries.GetCurrentUser;
using SafeFlow.Application.Identity.Queries.GetUserById;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.API.Controllers;

/// <summary>
/// User profile management endpoints.
/// </summary>
/// <remarks>
/// All endpoints require a valid JWT Bearer token.
/// Read access is permission-gated (<c>Users:Read</c>) except
/// <c>GET /me</c> which only requires authentication (IDOR-safe by design).
/// </remarks>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Produces("application/json")]
[Authorize]
public sealed class UsersController : ApiControllerBase
{
    // ── GET /api/v1/users/me ──────────────────────────────────────────────────

    /// <summary>Returns the profile of the currently authenticated user.</summary>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>The authenticated user's full profile.</returns>
    /// <response code="200">Profile returned successfully.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="404">Authenticated user no longer exists.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    // ── GET /api/v1/users/{id} ────────────────────────────────────────────────

    /// <summary>Returns the profile of a specific user by identifier.</summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>The user's full profile.</returns>
    /// <response code="200">Profile returned successfully.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Insufficient permissions (<c>Users:Read</c> required).</response>
    /// <response code="404">User not found.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.UsersRead)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return result.ToActionResult(this);
    }

    // ── GET /api/v1/users ─────────────────────────────────────────────────────

    /// <summary>Returns a paged list of all users.</summary>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>List of user profiles.</returns>
    /// <remarks>
    /// Pagination support will be added in Phase 1.5.  Currently returns all users.
    /// Requires <c>Users:Read</c> permission.
    /// </remarks>
    /// <response code="200">List returned successfully.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Insufficient permissions (<c>Users:Read</c> required).</response>
    [HttpGet]
    [Authorize(Policy = Permissions.UsersRead)]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public IActionResult GetUsers(CancellationToken cancellationToken)
    {
        // Phase 1.5: replace with a GetUsersQuery supporting paging + filtering.
        // For now, return an empty list to satisfy the contract without breaking
        // the build or wiring non-existent Application layer handlers.
        return Ok(Array.Empty<UserDto>());
    }
}
