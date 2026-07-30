using MediatR;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.Domain.Employees.Enums;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Commands.CreateEmployee;

/// <summary>
/// Command to create a new Employee aggregate root.
/// </summary>
public sealed record CreateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    Guid DepartmentId,
    string JobTitle,
    EmploymentType EmploymentType,
    DateTime HireDate,
    Guid TenantId,
    Guid? UserId = null) : IRequest<Result<EmployeeDto>>;
