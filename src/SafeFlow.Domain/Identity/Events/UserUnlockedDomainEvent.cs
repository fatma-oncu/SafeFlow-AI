using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Identity.Events;

/// <summary>
/// Raised when a previously locked <c>User</c> account is unlocked, restoring
/// the ability to authenticate.
/// </summary>
/// <param name="UserId">The unique identifier of the unlocked user.</param>
/// <param name="OccurredAt">The UTC instant at which the event occurred.</param>
public sealed record UserUnlockedDomainEvent(
    Guid UserId,
    DateTime OccurredAt) : IDomainEvent;
