using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.RiskAssessments.Events;

/// <summary>
/// Domain Event raised when a control measure is removed from a hazard in a <see cref="RiskAssessment"/>.
/// </summary>
public sealed record ControlMeasureRemovedDomainEvent(
    RiskAssessment RiskAssessment,
    Guid HazardId,
    Guid ControlMeasureId) : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
