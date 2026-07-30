using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Employees.Events;

/// <summary>
/// Raised when a new <c>Employee</c> aggregate is created.
/// </summary>
public sealed record EmployeeCreatedDomainEvent(
    Guid EmployeeId,
    string EmployeeNumber,
    string Email,
    Guid DepartmentId,
    DateTime OccurredAt) : IDomainEvent;
