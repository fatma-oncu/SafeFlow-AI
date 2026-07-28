using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeFlow.API.Authorization;
using SafeFlow.API.Extensions;
using SafeFlow.API.Models.Roles;
using SafeFlow.Application.Identity.Commands.AssignRole;
using SafeFlow.Application.Identity.Commands.RemoveRole;
using SafeFlow.Application.Identity.DTOs;
using SafeFlow.Application.Identity.Queries.GetRoles;

namespace SafeFlow.API.Controllers;

/// <summary>
/// Role catalogue and user-role assignment endpoints.
/// </summary>
/// <remarks>
/// All endpoints require a valid JWT Bearer token.
/// Role assignment and revocation require <c>Roles:Assign</c> and
/// <c>Roles:Revoke</c> permissions respectively.
/// </remarks>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Produces("application/json")]
[Authorize]
public sealed class RolesController : ApiControllerBase
{
    // ── GET /api/v1/roles ─────────────────────────────────────────────────────

    /// <summary>Returns all roles defined in the system.</summary>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>List of all roles.</returns>
    /// <response code="200">Roles returned successfully.</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Insufficient permissions (<c>Roles:Read</c> required).</response>
    [HttpGet("roles")]
    [Authorize(Policy = Permissions.RolesRead)]
    [ProducesResponseType(typeof(IReadOnlyList<RoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetRolesQuery(), cancellationToken);
        return result.ToActionResult(this);
    }

    // ── POST /api/v1/users/{id}/roles ─────────────────────────────────────────

    /// <summary>Assigns a role to a user.</summary>
    /// <param name="id">The identifier of the user receiving the role.</param>
    /// <param name="request">The role to assign.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <response code="204">Role assigned successfully (or user already held it).</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Insufficient permissions (<c>Roles:Assign</c> required).</response>
    /// <response code="404">User or role not found.</response>
    [HttpPost("users/{id:guid}/roles")]
    [Authorize(Policy = Permissions.RolesAssign)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(
        [FromRoute] Guid id,
        [FromBody]  AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignRoleCommand(UserId: id, RoleId: request.RoleId);
        var result  = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }

    // ── DELETE /api/v1/users/{id}/roles/{role} ────────────────────────────────

    /// <summary>Removes a role from a user.</summary>
    /// <param name="id">The identifier of the user.</param>
    /// <param name="role">The identifier of the role to remove.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <response code="204">Role removed successfully (or user did not hold it).</response>
    /// <response code="401">Not authenticated.</response>
    /// <response code="403">Insufficient permissions (<c>Roles:Revoke</c> required).</response>
    /// <response code="404">User or role not found.</response>
    [HttpDelete("users/{id:guid}/roles/{role:guid}")]
    [Authorize(Policy = Permissions.RolesRevoke)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(
        [FromRoute] Guid id,
        [FromRoute] Guid role,
        CancellationToken cancellationToken)
    {
        var command = new RemoveRoleCommand(UserId: id, RoleId: role);
        var result  = await Mediator.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
}
