using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Commands.DeactivateEmployee;

public sealed class DeactivateEmployeeCommandHandler(
    IRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeactivateEmployeeCommandHandler> logger)
    : IRequestHandler<DeactivateEmployeeCommand, Result>
{
    public async Task<Result> Handle(
        DeactivateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var spec = new EmployeeByIdSpecification(command.Id, trackForUpdate: true);
        var employee = await employeeRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        employee.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deactivated employee {EmployeeId}", command.Id);

        return Result.Success();
    }
}
