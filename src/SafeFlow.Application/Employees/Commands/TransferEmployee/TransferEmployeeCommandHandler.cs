using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.Employees.ValueObjects;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Commands.TransferEmployee;

public sealed class TransferEmployeeCommandHandler(
    IRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    ILogger<TransferEmployeeCommandHandler> logger)
    : IRequestHandler<TransferEmployeeCommand, Result<EmployeeDto>>
{
    public async Task<Result<EmployeeDto>> Handle(
        TransferEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var spec = new EmployeeByIdSpecification(command.Id, trackForUpdate: true);
        var employee = await employeeRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (employee is null)
        {
            return Result.Failure<EmployeeDto>(EmployeeErrors.NotFound);
        }

        // Concurrency Check
        if (command.RowVersion is not null && command.RowVersion.Length > 0 &&
            employee.RowVersion is not null && employee.RowVersion.Length > 0 &&
            !command.RowVersion.SequenceEqual(employee.RowVersion))
        {
            return Result.Failure<EmployeeDto>(EmployeeErrors.ConcurrencyConflict);
        }

        if (command.NewDepartmentId == Guid.Empty)
        {
            return Result.Failure<EmployeeDto>(EmployeeErrors.InvalidDepartment);
        }

        var newDeptId = DepartmentId.Create(command.NewDepartmentId);
        employee.TransferDepartment(newDeptId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Transferred employee {EmployeeId} to department {DepartmentId}", command.Id, command.NewDepartmentId);

        return Result.Success(EmployeeDto.FromAggregate(employee));
    }
}
