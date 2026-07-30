using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Commands.DeactivateEmployee;

public sealed record DeactivateEmployeeCommand(Guid Id) : IRequest<Result>;
