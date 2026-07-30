using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Commands.ActivateEmployee;

public sealed class ActivateEmployeeCommandHandler(
    IRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    ILogger<ActivateEmployeeCommandHandler> logger)
    : IRequestHandler<ActivateEmployeeCommand, Result>
{
    public async Task<Result> Handle(
        ActivateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var spec = new EmployeeByIdSpecification(command.Id, trackForUpdate: true);
        var employee = await employeeRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        employee.Activate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Activated employee {EmployeeId}", command.Id);

        return Result.Success();
    }
}
