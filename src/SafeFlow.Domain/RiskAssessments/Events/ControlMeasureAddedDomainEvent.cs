using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Domain.RiskAssessments.Entities;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.RiskAssessments.Events;

/// <summary>
/// Domain Event raised when a control measure is added to a hazard in a <see cref="RiskAssessment"/>.
/// </summary>
public sealed record ControlMeasureAddedDomainEvent(
    RiskAssessment RiskAssessment,
    Guid HazardId,
    RiskControlMeasure ControlMeasure) : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
