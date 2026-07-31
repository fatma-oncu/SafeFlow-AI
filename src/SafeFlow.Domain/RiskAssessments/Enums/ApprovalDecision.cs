namespace SafeFlow.Domain.RiskAssessments.Enums;

/// <summary>
/// Represents the decision type in a risk assessment approval workflow entry.
/// </summary>
public enum ApprovalDecision
{
    /// <summary>Assessment submitted for review.</summary>
    Submitted = 1,

    /// <summary>Assessment approved by reviewer.</summary>
    Approved = 2,

    /// <summary>Assessment rejected with comments.</summary>
    Rejected = 3
}
