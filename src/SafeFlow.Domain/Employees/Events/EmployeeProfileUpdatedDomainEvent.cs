using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Employees.Events;

/// <summary>
/// Raised when an <c>Employee</c> profile details are updated.
/// </summary>
public sealed record EmployeeProfileUpdatedDomainEvent(
    Guid EmployeeId,
    string FirstName,
    string LastName,
    string Email,
    DateTime OccurredAt) : IDomainEvent;
