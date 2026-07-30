using MediatR;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Commands.TransferEmployee;

public sealed record TransferEmployeeCommand(
    Guid Id,
    Guid NewDepartmentId,
    byte[] RowVersion) : IRequest<Result<EmployeeDto>>;
