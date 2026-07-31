using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.RiskAssessments.Events;

/// <summary>
/// Domain Event raised when a new <see cref="RiskAssessment"/> aggregate root is created.
/// </summary>
public sealed record RiskAssessmentCreatedDomainEvent(RiskAssessment RiskAssessment) : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
