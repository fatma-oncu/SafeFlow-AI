using MediatR;
using Microsoft.Extensions.Logging;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.Application.Employees.Interfaces;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.Domain.Employees.ValueObjects;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Application.Employees.Commands.CreateEmployee;

public sealed class CreateEmployeeCommandHandler(
    IRepository<Employee> employeeRepository,
    IEmployeeNumberGenerator numberGenerator,
    IUnitOfWork unitOfWork,
    ILogger<CreateEmployeeCommandHandler> logger)
    : IRequestHandler<CreateEmployeeCommand, Result<EmployeeDto>>
{
    public async Task<Result<EmployeeDto>> Handle(
        CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating new employee for email {Email}", command.Email);

        // 1. Email Uniqueness Check
        var existingEmailSpec = new EmployeeByEmailSpecification(command.Email);
        var existingEmailEmp = await employeeRepository.FirstOrDefaultAsync(existingEmailSpec, cancellationToken);
        if (existingEmailEmp is not null)
        {
            return Result.Failure<EmployeeDto>(EmployeeErrors.EmailAlreadyExists);
        }

        // 2. Generate Employee Number
        string generatedNumber = await numberGenerator.GenerateNextNumberAsync(cancellationToken);
        var numberSpec = new EmployeeByNumberSpecification(generatedNumber);
        var existingNumberEmp = await employeeRepository.FirstOrDefaultAsync(numberSpec, cancellationToken);
        if (existingNumberEmp is not null)
        {
            return Result.Failure<EmployeeDto>(EmployeeErrors.NumberAlreadyExists);
        }

        // 3. Construct Value Objects
        var empNumber = EmployeeNumber.Create(generatedNumber);
        var email = Email.Create(command.Email);
        var phone = !string.IsNullOrWhiteSpace(command.PhoneNumber)
            ? PhoneNumber.Create(command.PhoneNumber)
            : null;
        var deptId = DepartmentId.Create(command.DepartmentId);
        var jobTitle = JobTitle.Create(command.JobTitle);

        // 4. Instantiate Aggregate
        var employee = Employee.Create(
            empNumber,
            command.FirstName,
            command.LastName,
            email,
            phone,
            deptId,
            jobTitle,
            command.EmploymentType,
            command.HireDate,
            command.TenantId,
            command.UserId);

        await employeeRepository.AddAsync(employee, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully created employee {EmployeeId} ({EmployeeNumber})", employee.Id, employee.EmployeeNumber.Value);

        return Result.Success(EmployeeDto.FromAggregate(employee));
    }
}
