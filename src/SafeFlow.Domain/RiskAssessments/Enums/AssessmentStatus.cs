namespace SafeFlow.Domain.RiskAssessments.Enums;

/// <summary>
/// Represents the state lifecycle of a Risk Assessment.
/// </summary>
public enum AssessmentStatus
{
    /// <summary>Initial draft state — editable by creator.</summary>
    Draft = 1,

    /// <summary>Submitted for approval review.</summary>
    InReview = 2,

    /// <summary>Approved active status — operational.</summary>
    Approved = 3,

    /// <summary>Rejected after review — returned to draft for corrections.</summary>
    Rejected = 4,

    /// <summary>Archived status — historical read-only record.</summary>
    Archived = 5
}
