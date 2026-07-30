using MediatR;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Queries.SearchEmployees;

public sealed class SearchEmployeesQuery : PagedQuery, IRequest<Result<PagedResult<EmployeeSearchResultDto>>>
{
    public SearchEmployeesQuery(
        string? searchTerm = null,
        Guid? departmentId = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 20)
        : base(page, pageSize)
    {
        SearchTerm = searchTerm;
        DepartmentId = departmentId;
        IsActive = isActive;
    }

    public string? SearchTerm { get; }
    public Guid? DepartmentId { get; }
    public bool? IsActive { get; }
}
