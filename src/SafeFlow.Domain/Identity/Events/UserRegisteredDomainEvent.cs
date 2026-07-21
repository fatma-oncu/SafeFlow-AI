using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Identity.Events;

/// <summary>
/// Raised when a new <c>User</c> aggregate is successfully created.
/// Consumers use this event to send a welcome email, provision tenant resources,
/// or write to an audit log.
/// </summary>
/// <param name="UserId">The unique identifier of the newly registered user.</param>
/// <param name="Email">The normalised email address of the new user.</param>
/// <param name="OccurredAt">The UTC instant at which the event occurred.</param>
public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    DateTime OccurredAt) : IDomainEvent;
