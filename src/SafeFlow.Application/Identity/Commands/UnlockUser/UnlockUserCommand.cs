using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Commands.UnlockUser;

/// <summary>
/// Unlocks a previously locked user account, restoring the ability to authenticate.
/// </summary>
/// <param name="UserId">The unique identifier of the user to unlock.</param>
public sealed record UnlockUserCommand(Guid UserId) : IRequest<Result>;
