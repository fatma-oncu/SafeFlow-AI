using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.Employees.Enums;

namespace SafeFlow.Application.Employees.DTOs;

/// <summary>
/// Lightweight DTO for employee list items.
/// </summary>
public sealed record EmployeeListItemDto(
    Guid Id,
    string EmployeeNumber,
    string FullName,
    string Email,
    Guid DepartmentId,
    string JobTitle,
    EmploymentType EmploymentType,
    EmploymentStatus EmploymentStatus,
    bool IsActive)
{
    public static EmployeeListItemDto FromAggregate(Employee employee) => new(
        employee.Id,
        employee.EmployeeNumber.Value,
        employee.FullName,
        employee.Email.Value,
        employee.DepartmentId.Value,
        employee.JobTitle.Value,
        employee.EmploymentType,
        employee.EmploymentStatus,
        employee.IsActive);
}
