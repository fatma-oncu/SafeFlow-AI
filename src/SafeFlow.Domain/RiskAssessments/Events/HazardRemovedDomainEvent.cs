using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.RiskAssessments.Events;

/// <summary>
/// Domain Event raised when a hazard is removed from a <see cref="RiskAssessment"/>.
/// </summary>
public sealed record HazardRemovedDomainEvent(
    RiskAssessment RiskAssessment,
    Guid HazardId) : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
