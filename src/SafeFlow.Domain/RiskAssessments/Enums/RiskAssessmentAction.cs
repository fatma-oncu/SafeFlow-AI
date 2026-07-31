namespace SafeFlow.Domain.RiskAssessments.Enums;

/// <summary>
/// Represents the action type recorded in a <see cref="Entities.RiskAssessmentHistory"/> audit log entry.
/// </summary>
public enum RiskAssessmentAction
{
    /// <summary>Assessment created.</summary>
    Created = 1,

    /// <summary>Header details updated.</summary>
    DetailsUpdated = 2,

    /// <summary>Hazard added.</summary>
    HazardAdded = 3,

    /// <summary>Hazard updated.</summary>
    HazardUpdated = 4,

    /// <summary>Hazard removed.</summary>
    HazardRemoved = 5,

    /// <summary>Control measure added.</summary>
    ControlMeasureAdded = 6,

    /// <summary>Control measure removed.</summary>
    ControlMeasureRemoved = 7,

    /// <summary>Submitted for review.</summary>
    SubmittedForReview = 8,

    /// <summary>Approved by reviewer.</summary>
    Approved = 9,

    /// <summary>Rejected by reviewer.</summary>
    Rejected = 10,

    /// <summary>Archived.</summary>
    Archived = 11,

    /// <summary>New revision created.</summary>
    RevisionCreated = 12,

    /// <summary>Soft deleted.</summary>
    SoftDeleted = 13
}
