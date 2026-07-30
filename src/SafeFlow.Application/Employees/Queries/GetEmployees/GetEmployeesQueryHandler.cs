using MediatR;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Queries.GetEmployees;

public sealed class GetEmployeesQueryHandler(
    IReadRepository<Employee> employeeRepository)
    : IRequestHandler<GetEmployeesQuery, Result<PagedResult<EmployeeListItemDto>>>
{
    public async Task<Result<PagedResult<EmployeeListItemDto>>> Handle(
        GetEmployeesQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new EmployeePagedSpecification(query.Page, query.PageSize, query.DepartmentId, query.IsActive);
        var countSpec = new EmployeePagedCountSpecification(query.DepartmentId, query.IsActive);

        var employees = await employeeRepository.ListAsync(spec, cancellationToken);
        int totalCount = await employeeRepository.CountAsync(countSpec, cancellationToken);

        var dtos = employees.Select(EmployeeListItemDto.FromAggregate).ToList();
        var pagedResult = PagedResult<EmployeeListItemDto>.Create(dtos, totalCount, query.Page, query.PageSize);

        return Result.Success(pagedResult);
    }
}
