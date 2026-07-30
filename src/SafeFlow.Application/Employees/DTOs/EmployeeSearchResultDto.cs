using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.Employees.Enums;

namespace SafeFlow.Application.Employees.DTOs;

/// <summary>
/// DTO returned for employee search queries.
/// </summary>
public sealed record EmployeeSearchResultDto(
    Guid Id,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    Guid DepartmentId,
    string JobTitle,
    EmploymentStatus EmploymentStatus,
    bool IsActive)
{
    public static EmployeeSearchResultDto FromAggregate(Employee employee) => new(
        employee.Id,
        employee.EmployeeNumber.Value,
        employee.FirstName,
        employee.LastName,
        employee.FullName,
        employee.Email.Value,
        employee.DepartmentId.Value,
        employee.JobTitle.Value,
        employee.EmploymentStatus,
        employee.IsActive);
}
