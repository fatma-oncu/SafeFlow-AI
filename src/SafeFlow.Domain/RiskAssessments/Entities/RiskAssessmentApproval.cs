using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.SharedKernel.Entities;

namespace SafeFlow.Domain.RiskAssessments.Entities;

/// <summary>
/// Represents an immutable audit trail entry for a risk assessment approval decision.
/// Critical for ISO 45001 and enterprise compliance audits.
/// </summary>
public sealed class RiskAssessmentApproval : BaseEntity
{
    private RiskAssessmentApproval() { }

    /// <summary>Gets the parent risk assessment identifier.</summary>
    public Guid RiskAssessmentId { get; private set; }

    /// <summary>Gets the identifier of the employee performing the approval decision.</summary>
    public Guid EmployeeId { get; private set; }

    /// <summary>Gets the decision type (Submitted, Approved, Rejected).</summary>
    public ApprovalDecision Decision { get; private set; }

    /// <summary>Gets the optional review/approval comment.</summary>
    public string? Comment { get; private set; }

    /// <summary>Gets the timestamp when the decision was recorded.</summary>
    public DateTime ProcessedAt { get; private set; }

    internal static RiskAssessmentApproval Create(
        Guid assessmentId,
        Guid employeeId,
        ApprovalDecision decision,
        string? comment)
    {
        if (assessmentId == Guid.Empty)
            throw new ArgumentException("RiskAssessmentId must not be empty.", nameof(assessmentId));

        if (employeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId must not be empty.", nameof(employeeId));

        return new RiskAssessmentApproval
        {
            Id = Guid.NewGuid(),
            RiskAssessmentId = assessmentId,
            EmployeeId = employeeId,
            Decision = decision,
            Comment = comment?.Trim(),
            ProcessedAt = DateTime.UtcNow
        };
    }
}
