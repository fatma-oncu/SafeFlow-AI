using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Employees.Events;

/// <summary>
/// Raised when an <c>Employee</c> is transferred to a new department.
/// </summary>
public sealed record EmployeeTransferredDomainEvent(
    Guid EmployeeId,
    Guid OldDepartmentId,
    Guid NewDepartmentId,
    DateTime OccurredAt) : IDomainEvent;
