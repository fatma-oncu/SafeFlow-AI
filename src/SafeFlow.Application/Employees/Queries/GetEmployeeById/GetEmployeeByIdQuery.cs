using MediatR;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Queries.GetEmployeeById;

public sealed record GetEmployeeByIdQuery(Guid Id) : IRequest<Result<EmployeeDto>>;
