using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.Employees.Specifications;

/// <summary>
/// Retrieves a paged list of active/non-deleted <see cref="Employee"/> aggregates with optional department and status filters.
/// </summary>
public sealed class EmployeePagedSpecification : BaseSpecification<Employee>
{
    public EmployeePagedSpecification(
        int page,
        int pageSize,
        Guid? departmentId = null,
        bool? isActive = null)
        : base(e => (!departmentId.HasValue || e.DepartmentId.Value == departmentId.Value) &&
                    (!isActive.HasValue || e.IsActive == isActive.Value))
    {
        ApplyOrderBy(e => e.LastName);
        ApplyPaging((page - 1) * pageSize, pageSize);
        ApplyNoTracking();
    }
}

/// <summary>
/// Specification for counting total records matching department and status filters without paging.
/// </summary>
public sealed class EmployeePagedCountSpecification : BaseSpecification<Employee>
{
    public EmployeePagedCountSpecification(
        Guid? departmentId = null,
        bool? isActive = null)
        : base(e => (!departmentId.HasValue || e.DepartmentId.Value == departmentId.Value) &&
                    (!isActive.HasValue || e.IsActive == isActive.Value))
    {
        ApplyNoTracking();
    }
}
