using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Identity.Events;

/// <summary>
/// Raised when a <c>Role</c> is removed from a <c>User</c>.
/// Consumers may invalidate cached permission sets or audit the de-authorisation.
/// </summary>
/// <param name="UserId">The unique identifier of the user who lost the role.</param>
/// <param name="RoleId">The unique identifier of the role that was removed.</param>
/// <param name="OccurredAt">The UTC instant at which the event occurred.</param>
public sealed record UserRoleRemovedDomainEvent(
    Guid UserId,
    Guid RoleId,
    DateTime OccurredAt) : IDomainEvent;
