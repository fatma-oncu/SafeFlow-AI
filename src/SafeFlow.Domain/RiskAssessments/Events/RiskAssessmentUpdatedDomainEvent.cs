using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.RiskAssessments.Events;

/// <summary>
/// Domain Event raised when details of a <see cref="RiskAssessment"/> are updated.
/// </summary>
public sealed record RiskAssessmentUpdatedDomainEvent(RiskAssessment RiskAssessment) : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
