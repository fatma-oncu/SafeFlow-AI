namespace SafeFlow.Domain.Incidents.Enums;

/// <summary>
/// Status states for corrective actions assigned to resolve an incident.
/// </summary>
public enum CorrectiveActionStatus
{
    /// <summary>Action assigned but work not yet started.</summary>
    Open = 1,

    /// <summary>Work actively in progress.</summary>
    InProgress = 2,

    /// <summary>Action completed by assigned party.</summary>
    Completed = 3,

    /// <summary>Completion verified by safety manager.</summary>
    Verified = 4
}
