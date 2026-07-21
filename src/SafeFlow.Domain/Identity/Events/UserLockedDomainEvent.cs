using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Identity.Events;

/// <summary>
/// Raised when a <c>User</c> account is locked, preventing further authentication.
/// Consumers may notify security teams or send an alert to the user.
/// </summary>
/// <param name="UserId">The unique identifier of the locked user.</param>
/// <param name="Reason">A human-readable explanation of why the account was locked.</param>
/// <param name="OccurredAt">The UTC instant at which the event occurred.</param>
public sealed record UserLockedDomainEvent(
    Guid UserId,
    string Reason,
    DateTime OccurredAt) : IDomainEvent;
