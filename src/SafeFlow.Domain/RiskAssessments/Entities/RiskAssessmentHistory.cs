using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.SharedKernel.Entities;

namespace SafeFlow.Domain.RiskAssessments.Entities;

/// <summary>
/// Represents an immutable audit trail entry tracking lifecycle actions, status changes,
/// and actor interactions on a <see cref="Aggregates.RiskAssessment"/>.
/// Required for ISO 45001, ISO 9001, and corporate compliance auditing.
/// </summary>
public sealed class RiskAssessmentHistory : BaseEntity
{
    private RiskAssessmentHistory() { }

    /// <summary>Gets the parent risk assessment identifier.</summary>
    public Guid RiskAssessmentId { get; private set; }

    /// <summary>Gets the lifecycle action executed.</summary>
    public RiskAssessmentAction Action { get; private set; }

    /// <summary>Gets the identifier of the Employee who performed the action.</summary>
    public Guid PerformedByEmployeeId { get; private set; }

    /// <summary>Gets the prior assessment status before the action (if applicable).</summary>
    public AssessmentStatus? OldStatus { get; private set; }

    /// <summary>Gets the new assessment status resulting from the action.</summary>
    public AssessmentStatus NewStatus { get; private set; }

    /// <summary>Gets optional notes or review comments associated with the action.</summary>
    public string? Comment { get; private set; }

    internal static RiskAssessmentHistory Create(
        Guid assessmentId,
        RiskAssessmentAction action,
        Guid performedByEmployeeId,
        AssessmentStatus? oldStatus,
        AssessmentStatus newStatus,
        string? comment)
    {
        if (assessmentId == Guid.Empty)
            throw new ArgumentException("RiskAssessmentId must not be empty.", nameof(assessmentId));

        if (performedByEmployeeId == Guid.Empty)
            throw new ArgumentException("PerformedByEmployeeId must not be empty.", nameof(performedByEmployeeId));

        return new RiskAssessmentHistory
        {
            Id = Guid.NewGuid(),
            RiskAssessmentId = assessmentId,
            Action = action,
            PerformedByEmployeeId = performedByEmployeeId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Comment = comment?.Trim()
        };
    }
}
