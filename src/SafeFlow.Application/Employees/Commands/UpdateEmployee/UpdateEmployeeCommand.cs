using MediatR;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Commands.UpdateEmployee;

public sealed record UpdateEmployeeCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string JobTitle,
    byte[] RowVersion) : IRequest<Result<EmployeeDto>>;
