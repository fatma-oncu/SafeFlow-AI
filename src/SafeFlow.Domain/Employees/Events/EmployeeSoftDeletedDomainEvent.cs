using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Employees.Events;

/// <summary>
/// Raised when an <c>Employee</c> is soft deleted.
/// </summary>
public sealed record EmployeeSoftDeletedDomainEvent(
    Guid EmployeeId,
    DateTime OccurredAt) : IDomainEvent;
