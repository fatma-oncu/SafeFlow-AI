using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.RiskAssessments.Events;

/// <summary>
/// Domain Event raised when a <see cref="RiskAssessment"/> is submitted for review.
/// </summary>
public sealed record RiskAssessmentSubmittedForReviewDomainEvent(
    RiskAssessment RiskAssessment,
    Guid SubmittedByEmployeeId) : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
