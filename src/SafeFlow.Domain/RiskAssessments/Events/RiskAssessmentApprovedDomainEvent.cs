using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.RiskAssessments.Events;

/// <summary>
/// Domain Event raised when a <see cref="RiskAssessment"/> is approved.
/// </summary>
public sealed record RiskAssessmentApprovedDomainEvent(
    RiskAssessment RiskAssessment,
    Guid ApprovedByEmployeeId,
    string? Comment) : IDomainEvent
{
    /// <inheritdoc/>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
