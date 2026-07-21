using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Identity.Events;

/// <summary>
/// Raised when a <c>User</c>'s last login timestamp is updated following a
/// successful authentication.
/// Consumers may use this event to update activity dashboards or revoke
/// idle-timeout tokens.
/// </summary>
/// <param name="UserId">The unique identifier of the user who logged in.</param>
/// <param name="LoggedInAt">The UTC instant of the successful login.</param>
/// <param name="OccurredAt">The UTC instant at which the event occurred.</param>
public sealed record UserLoggedInDomainEvent(
    Guid UserId,
    DateTime LoggedInAt,
    DateTime OccurredAt) : IDomainEvent;
