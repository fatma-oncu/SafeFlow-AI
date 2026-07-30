using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.Employees.Enums;

namespace SafeFlow.Application.Employees.DTOs;

/// <summary>
/// Detailed DTO for an Employee aggregate root.
/// </summary>
public sealed record EmployeeDto(
    Guid Id,
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string? PhoneNumber,
    Guid DepartmentId,
    string JobTitle,
    EmploymentType EmploymentType,
    EmploymentStatus EmploymentStatus,
    DateTime HireDate,
    Guid? UserId,
    bool IsActive,
    Guid TenantId,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? LastModifiedAt,
    string? LastModifiedBy,
    byte[] RowVersion)
{
    public static EmployeeDto FromAggregate(Employee employee) => new(
        employee.Id,
        employee.EmployeeNumber.Value,
        employee.FirstName,
        employee.LastName,
        employee.FullName,
        employee.Email.Value,
        employee.PhoneNumber?.Value,
        employee.DepartmentId.Value,
        employee.JobTitle.Value,
        employee.EmploymentType,
        employee.EmploymentStatus,
        employee.HireDate,
        employee.UserId,
        employee.IsActive,
        employee.TenantId,
        employee.CreatedAt,
        employee.CreatedBy,
        employee.LastModifiedAt,
        employee.LastModifiedBy,
        employee.RowVersion);
}
