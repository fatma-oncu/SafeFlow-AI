using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.RiskAssessments.Specifications;

/// <summary>
/// Specification for fetching risk assessments belonging to a specific department.
/// </summary>
public sealed class RiskAssessmentByDepartmentSpecification : BaseSpecification<RiskAssessment>
{
    public RiskAssessmentByDepartmentSpecification(Guid departmentId)
        : base(r => r.DepartmentId == departmentId)
    {
        ApplyOrderByDescending(r => r.CreatedAt);
        ApplyNoTracking();
    }
}
