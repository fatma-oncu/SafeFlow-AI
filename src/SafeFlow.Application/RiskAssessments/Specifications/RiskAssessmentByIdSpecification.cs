using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.RiskAssessments.Specifications;

/// <summary>
/// Specification for fetching a <see cref="RiskAssessment"/> aggregate by ID.
/// Includes hazards, controls, approval history, and audit log entries.
/// </summary>
public sealed class RiskAssessmentByIdSpecification : BaseSpecification<RiskAssessment>
{
    public RiskAssessmentByIdSpecification(Guid id)
        : base(r => r.Id == id)
    {
        AddInclude(r => r.Hazards);
        AddInclude(r => r.Approvals);
        AddInclude(r => r.History);
    }
}
