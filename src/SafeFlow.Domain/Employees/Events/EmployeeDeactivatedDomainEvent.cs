using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Employees.Events;

/// <summary>
/// Raised when an <c>Employee</c> is deactivated.
/// </summary>
public sealed record EmployeeDeactivatedDomainEvent(
    Guid EmployeeId,
    DateTime OccurredAt) : IDomainEvent;
