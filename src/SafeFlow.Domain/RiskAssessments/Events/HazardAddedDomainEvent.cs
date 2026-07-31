using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Domain.RiskAssessments.Entities;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.RiskAssessments.Events;

/// <summary>
/// Domain Event raised when a hazard is added to a <see cref="RiskAssessment"/>.
/// </summary>
public sealed record HazardAddedDomainEvent(
    RiskAssessment RiskAssessment,
    RiskHazard Hazard) : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
