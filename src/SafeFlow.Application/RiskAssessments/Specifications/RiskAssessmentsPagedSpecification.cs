using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.RiskAssessments.Specifications;

/// <summary>
/// Specification for returning a paged list of risk assessments with optional department and status filtering.
/// </summary>
public sealed class RiskAssessmentsPagedSpecification : BaseSpecification<RiskAssessment>
{
    public RiskAssessmentsPagedSpecification(
        int page,
        int pageSize,
        Guid? departmentId = null,
        AssessmentStatus? status = null)
        : base(r =>
            (!departmentId.HasValue || r.DepartmentId == departmentId.Value) &&
            (!status.HasValue || r.Status == status.Value))
    {
        ApplyOrderByDescending(r => r.CreatedAt);
        ApplyPaging((page - 1) * pageSize, pageSize);
        ApplyNoTracking();
    }
}
