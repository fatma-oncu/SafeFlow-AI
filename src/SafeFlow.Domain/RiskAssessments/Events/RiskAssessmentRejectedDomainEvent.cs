using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.RiskAssessments.Events;

/// <summary>
/// Domain Event raised when a <see cref="RiskAssessment"/> is rejected.
/// </summary>
public sealed record RiskAssessmentRejectedDomainEvent(
    RiskAssessment RiskAssessment,
    Guid RejectedByEmployeeId,
    string Comment) : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
