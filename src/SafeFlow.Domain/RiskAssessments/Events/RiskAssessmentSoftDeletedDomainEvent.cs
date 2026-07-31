using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.RiskAssessments.Events;

/// <summary>
/// Domain Event raised when a <see cref="RiskAssessment"/> is soft-deleted.
/// </summary>
public sealed record RiskAssessmentSoftDeletedDomainEvent(RiskAssessment RiskAssessment) : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
