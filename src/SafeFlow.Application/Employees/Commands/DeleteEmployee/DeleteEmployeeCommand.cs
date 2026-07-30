using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Commands.DeleteEmployee;

public sealed record DeleteEmployeeCommand(Guid Id, string? DeletedBy = null) : IRequest<Result>;
