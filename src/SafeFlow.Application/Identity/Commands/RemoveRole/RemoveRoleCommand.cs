using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.RemoveRole;

/// <summary>
/// Removes the given role from a user.
/// Idempotent: removing a role the user does not hold has no effect.
/// </summary>
/// <param name="UserId">The identifier of the user losing the role.</param>
/// <param name="RoleId">The identifier of the role to remove.</param>
public sealed record RemoveRoleCommand(
    Guid UserId,
    Guid RoleId) : IRequest<Result>;
