using SafeFlow.Domain.Employees.Aggregates;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.Employees.Specifications;

/// <summary>
/// Retrieves an <see cref="Employee"/> by email address.
/// </summary>
public sealed class EmployeeByEmailSpecification : BaseSpecification<Employee>
{
    public EmployeeByEmailSpecification(string email)
        : base(e => e.Email.Value == email.ToLowerInvariant().Trim())
    {
        ApplyNoTracking();
    }
}
