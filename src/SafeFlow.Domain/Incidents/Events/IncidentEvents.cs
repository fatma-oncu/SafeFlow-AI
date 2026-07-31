using SafeFlow.Domain.Incidents.Aggregates;
using SafeFlow.Domain.Incidents.Entities;
using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.SharedKernel.Events;

namespace SafeFlow.Domain.Incidents.Events;

/// <summary>Raised when an incident is newly reported.</summary>
public sealed record IncidentReportedDomainEvent(Incident Incident) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>Raised when an incident is assigned to an investigator.</summary>
public sealed record IncidentAssignedDomainEvent(Incident Incident, Guid AssignedToEmployeeId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>Raised when investigation commences on an incident.</summary>
public sealed record IncidentInvestigationStartedDomainEvent(Incident Incident, Guid InvestigatorEmployeeId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>Raised when an incident investigation is resolved.</summary>
public sealed record IncidentResolvedDomainEvent(Incident Incident, InvestigationResult Result) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>Raised when an incident is formally closed.</summary>
public sealed record IncidentClosedDomainEvent(Incident Incident, Guid ClosedByEmployeeId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>Raised when an incident is cancelled.</summary>
public sealed record IncidentCancelledDomainEvent(Incident Incident, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>Raised when a closed or cancelled incident is reopened.</summary>
public sealed record IncidentReopenedDomainEvent(Incident Incident, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>Raised when a corrective action is added to an incident.</summary>
public sealed record CorrectiveActionAddedDomainEvent(Incident Incident, CorrectiveAction CorrectiveAction) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>Raised when a corrective action is completed.</summary>
public sealed record CorrectiveActionCompletedDomainEvent(Incident Incident, CorrectiveAction CorrectiveAction) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>Raised when a comment is added to an incident.</summary>
public sealed record CommentAddedDomainEvent(Incident Incident, IncidentComment Comment) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
