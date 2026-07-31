namespace SafeFlow.Domain.Incidents.Enums;

/// <summary>
/// Represents the lifecycle states of an Incident.
/// Workflow: Reported -> Assigned -> UnderInvestigation -> WaitingCorrectiveAction -> Resolved -> Closed (or Cancelled/Reopened).
/// </summary>
public enum IncidentStatus
{
    /// <summary>Incident has been reported and logged into system.</summary>
    Reported = 1,

    /// <summary>Incident has been assigned to a responsible investigator.</summary>
    Assigned = 2,

    /// <summary>Investigation is actively underway.</summary>
    UnderInvestigation = 3,

    /// <summary>Investigation complete; awaiting completion of assigned corrective actions.</summary>
    WaitingCorrectiveAction = 4,

    /// <summary>Corrective actions completed; incident resolved.</summary>
    Resolved = 5,

    /// <summary>Incident formally closed after review.</summary>
    Closed = 6,

    /// <summary>Incident report cancelled or invalidated.</summary>
    Cancelled = 7
}
