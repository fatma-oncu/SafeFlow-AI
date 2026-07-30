using MediatR;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Commands.ActivateEmployee;

public sealed record ActivateEmployeeCommand(Guid Id) : IRequest<Result>;
