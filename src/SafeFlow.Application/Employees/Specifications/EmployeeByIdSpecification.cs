using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.Employees.Specifications;

/// <summary>
/// Retrieves an <see cref="Employee"/> by its primary key ID.
/// </summary>
public sealed class EmployeeByIdSpecification : BaseSpecification<Employee>
{
    public EmployeeByIdSpecification(Guid employeeId, bool trackForUpdate = false)
        : base(e => e.Id == employeeId)
    {
        if (!trackForUpdate)
            ApplyNoTracking();
    }
}
