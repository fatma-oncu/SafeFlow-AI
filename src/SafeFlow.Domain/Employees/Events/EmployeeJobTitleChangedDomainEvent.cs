using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Employees.Events;

/// <summary>
/// Raised when an <c>Employee</c> job title is changed.
/// </summary>
public sealed record EmployeeJobTitleChangedDomainEvent(
    Guid EmployeeId,
    string OldJobTitle,
    string NewJobTitle,
    DateTime OccurredAt) : IDomainEvent;
