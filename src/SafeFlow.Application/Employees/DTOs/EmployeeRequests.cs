using SafeFlow.Domain.Employees.Enums;

namespace SafeFlow.Application.Employees.DTOs;

/// <summary>
/// HTTP Request contract models for Employee API endpoints.
/// </summary>
public static class EmployeeRequests
{
    public sealed record CreateEmployeeRequest(
        string FirstName,
        string LastName,
        string Email,
        string? PhoneNumber,
        Guid DepartmentId,
        string JobTitle,
        EmploymentType EmploymentType,
        DateTime HireDate,
        Guid TenantId,
        Guid? UserId = null);

    public sealed record UpdateEmployeeRequest(
        string FirstName,
        string LastName,
        string Email,
        string? PhoneNumber,
        string JobTitle,
        byte[] RowVersion);

    public sealed record TransferEmployeeRequest(
        Guid NewDepartmentId,
        byte[] RowVersion);
}
