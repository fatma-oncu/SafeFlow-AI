using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.RiskAssessments.Specifications;

/// <summary>
/// Specification for fetching a <see cref="RiskAssessment"/> by its formatted assessment number.
/// </summary>
public sealed class RiskAssessmentByNumberSpecification : BaseSpecification<RiskAssessment>
{
    public RiskAssessmentByNumberSpecification(string number)
        : base(r => r.AssessmentNumber.Value == number.Trim())
    {
    }
}
