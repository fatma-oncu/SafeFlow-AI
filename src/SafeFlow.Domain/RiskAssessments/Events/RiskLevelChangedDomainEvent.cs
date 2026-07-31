using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.RiskAssessments.Events;

/// <summary>
/// Domain Event raised when the overall calculated risk level of a <see cref="RiskAssessment"/> changes.
/// </summary>
public sealed record RiskLevelChangedDomainEvent(
    RiskAssessment RiskAssessment,
    RiskLevel PreviousRiskLevel,
    RiskLevel NewRiskLevel) : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
