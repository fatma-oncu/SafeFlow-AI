using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Commands.DeleteEmployee;

public sealed class DeleteEmployeeCommandHandler(
    IRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteEmployeeCommandHandler> logger)
    : IRequestHandler<DeleteEmployeeCommand, Result>
{
    public async Task<Result> Handle(
        DeleteEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var spec = new EmployeeByIdSpecification(command.Id, trackForUpdate: true);
        var employee = await employeeRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (employee is null)
        {
            return Result.Failure(EmployeeErrors.NotFound);
        }

        employee.PerformSoftDelete(command.DeletedBy ?? "system");
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Soft deleted employee {EmployeeId}", command.Id);

        return Result.Success();
    }
}
