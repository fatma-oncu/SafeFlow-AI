using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.RiskAssessments.Specifications;

/// <summary>
/// Specification for filtering risk assessments by lifecycle status.
/// </summary>
public sealed class RiskAssessmentByStatusSpecification : BaseSpecification<RiskAssessment>
{
    public RiskAssessmentByStatusSpecification(AssessmentStatus status)
        : base(r => r.Status == status)
    {
        ApplyOrderByDescending(r => r.CreatedAt);
        ApplyNoTracking();
    }
}
