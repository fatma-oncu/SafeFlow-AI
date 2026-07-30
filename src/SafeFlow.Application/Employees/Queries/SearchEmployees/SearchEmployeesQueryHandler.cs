using MediatR;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.Application.Employees.Specifications;
using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.SharedKernel.Interfaces;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Queries.SearchEmployees;

public sealed class SearchEmployeesQueryHandler(
    IReadRepository<Employee> employeeRepository)
    : IRequestHandler<SearchEmployeesQuery, Result<PagedResult<EmployeeSearchResultDto>>>
{
    public async Task<Result<PagedResult<EmployeeSearchResultDto>>> Handle(
        SearchEmployeesQuery query,
        CancellationToken cancellationToken)
    {
        var spec = new EmployeeSearchSpecification(query.SearchTerm, query.DepartmentId, query.IsActive, query.Page, query.PageSize);
        var countSpec = new EmployeeSearchCountSpecification(query.SearchTerm, query.DepartmentId, query.IsActive);

        var employees = await employeeRepository.ListAsync(spec, cancellationToken);
        int totalCount = await employeeRepository.CountAsync(countSpec, cancellationToken);

        var dtos = employees.Select(EmployeeSearchResultDto.FromAggregate).ToList();
        var pagedResult = PagedResult<EmployeeSearchResultDto>.Create(dtos, totalCount, query.Page, query.PageSize);

        return Result.Success(pagedResult);
    }
}
