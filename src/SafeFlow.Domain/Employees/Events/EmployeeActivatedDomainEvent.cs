using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Employees.Events;

/// <summary>
/// Raised when an <c>Employee</c> is activated.
/// </summary>
public sealed record EmployeeActivatedDomainEvent(
    Guid EmployeeId,
    DateTime OccurredAt) : IDomainEvent;
