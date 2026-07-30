using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.Employees.ValueObjects;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Application.Employees.Commands.UpdateEmployee;

public sealed class UpdateEmployeeCommandHandler(
    IRepository<Employee> employeeRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateEmployeeCommandHandler> logger)
    : IRequestHandler<UpdateEmployeeCommand, Result<EmployeeDto>>
{
    public async Task<Result<EmployeeDto>> Handle(
        UpdateEmployeeCommand command,
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

        // Email Uniqueness Check if changed
        if (!string.Equals(employee.Email.Value, command.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailSpec = new EmployeeByEmailSpecification(command.Email);
            var existingWithEmail = await employeeRepository.FirstOrDefaultAsync(emailSpec, cancellationToken);
            if (existingWithEmail is not null && existingWithEmail.Id != employee.Id)
            {
                return Result.Failure<EmployeeDto>(EmployeeErrors.EmailAlreadyExists);
            }
        }

        var newEmail = Email.Create(command.Email);
        var newPhone = !string.IsNullOrWhiteSpace(command.PhoneNumber)
            ? PhoneNumber.Create(command.PhoneNumber)
            : null;
        var newJobTitle = JobTitle.Create(command.JobTitle);

        employee.UpdateProfile(command.FirstName, command.LastName, newEmail, newPhone);
        employee.ChangeJobTitle(newJobTitle);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated profile for employee {EmployeeId}", command.Id);

        return Result.Success(EmployeeDto.FromAggregate(employee));
    }
}
