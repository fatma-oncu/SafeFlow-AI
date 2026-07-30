using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.Employees.Specifications;

/// <summary>
/// Retrieves an <see cref="Employee"/> by its unique employee number.
/// </summary>
public sealed class EmployeeByNumberSpecification : BaseSpecification<Employee>
{
    public EmployeeByNumberSpecification(string employeeNumber)
        : base(e => e.EmployeeNumber.Value == employeeNumber.Trim())
    {
        ApplyNoTracking();
    }
}
