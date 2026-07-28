using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.LockUser;

/// <summary>
/// Locks a user account, preventing further authentication until unlocked.
/// Intended for administrative use.
/// </summary>
/// <param name="UserId">The unique identifier of the user to lock.</param>
/// <param name="Reason">A human-readable explanation for the lockout.</param>
public sealed record LockUserCommand(
    Guid UserId,
    string Reason) : IRequest<Result>;
