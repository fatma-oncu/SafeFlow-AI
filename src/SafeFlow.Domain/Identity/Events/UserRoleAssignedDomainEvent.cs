using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Identity.Events;

/// <summary>
/// Raised when a <c>Role</c> is assigned to a <c>User</c>.
/// Consumers may refresh cached permission sets or audit the authorisation change.
/// </summary>
/// <param name="UserId">The unique identifier of the user who received the role.</param>
/// <param name="RoleId">The unique identifier of the role that was assigned.</param>
/// <param name="OccurredAt">The UTC instant at which the event occurred.</param>
public sealed record UserRoleAssignedDomainEvent(
    Guid UserId,
    Guid RoleId,
    DateTime OccurredAt) : IDomainEvent;
