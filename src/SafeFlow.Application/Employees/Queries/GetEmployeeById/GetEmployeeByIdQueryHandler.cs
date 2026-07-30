using MediatR;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Queries.GetEmployeeById;

public sealed class GetEmployeeByIdQueryHandler(
    IReadRepository<Employee> employeeRepository)
    : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
{
    public async Task<Result<EmployeeDto>> Handle(
        GetEmployeeByIdQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new EmployeeByIdSpecification(query.Id, trackForUpdate: false);
        var employee = await employeeRepository.FirstOrDefaultAsync(spec, cancellationToken);
        if (employee is null)
        {
            return Result.Failure<EmployeeDto>(EmployeeErrors.NotFound);
        }

        return Result.Success(EmployeeDto.FromAggregate(employee));
    }
}
