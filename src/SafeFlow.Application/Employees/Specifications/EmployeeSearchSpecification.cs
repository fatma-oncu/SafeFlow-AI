using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.Employees.Specifications;

/// <summary>
/// Specification for searching employees across number, first name, last name, email, department, and status.
/// </summary>
public sealed class EmployeeSearchSpecification : BaseSpecification<Employee>
{
    public EmployeeSearchSpecification(
        string? searchTerm,
        Guid? departmentId,
        bool? isActive,
        int page = 1,
        int pageSize = 20)
        : base(e =>
            (string.IsNullOrWhiteSpace(searchTerm) ||
                e.EmployeeNumber.Value.Contains(searchTerm.Trim()) ||
                e.FirstName.Contains(searchTerm.Trim()) ||
                e.LastName.Contains(searchTerm.Trim()) ||
                e.Email.Value.Contains(searchTerm.Trim())) &&
            (!departmentId.HasValue || e.DepartmentId.Value == departmentId.Value) &&
            (!isActive.HasValue || e.IsActive == isActive.Value))
    {
        ApplyOrderBy(e => e.LastName);
        ApplyPaging((page - 1) * pageSize, pageSize);
        ApplyNoTracking();
    }
}

/// <summary>
/// Specification for counting total search results matching search filters without paging parameters.
/// </summary>
public sealed class EmployeeSearchCountSpecification : BaseSpecification<Employee>
{
    public EmployeeSearchCountSpecification(
        string? searchTerm,
        Guid? departmentId,
        bool? isActive)
        : base(e =>
            (string.IsNullOrWhiteSpace(searchTerm) ||
                e.EmployeeNumber.Value.Contains(searchTerm.Trim()) ||
                e.FirstName.Contains(searchTerm.Trim()) ||
                e.LastName.Contains(searchTerm.Trim()) ||
                e.Email.Value.Contains(searchTerm.Trim())) &&
            (!departmentId.HasValue || e.DepartmentId.Value == departmentId.Value) &&
            (!isActive.HasValue || e.IsActive == isActive.Value))
    {
        ApplyNoTracking();
    }
}
