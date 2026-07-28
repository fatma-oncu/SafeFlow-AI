using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.AssignRole;

/// <summary>
/// Assigns the given role to a user.
/// Idempotent: assigning a role the user already holds has no effect.
/// </summary>
/// <param name="UserId">The identifier of the user receiving the role.</param>
/// <param name="RoleId">The identifier of the role to assign.</param>
public sealed record AssignRoleCommand(
    Guid UserId,
    Guid RoleId) : IRequest<Result>;
