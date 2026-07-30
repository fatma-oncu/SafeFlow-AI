using MediatR;
using SafeFlow.Application.Employees.DTOs;
using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Employees.Queries.GetEmployees;

public sealed class GetEmployeesQuery : PagedQuery, IRequest<Result<PagedResult<EmployeeListItemDto>>>
{
    public GetEmployeesQuery(
        int page = 1,
        int pageSize = 20,
        Guid? departmentId = null,
        bool? isActive = null)
        : base(page, pageSize)
    {
        DepartmentId = departmentId;
        IsActive = isActive;
    }

    public Guid? DepartmentId { get; }
    public bool? IsActive { get; }
}
