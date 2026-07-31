using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.Domain.Incidents.ValueObjects;
using SafeFlow.SharedKernel.Entities;

namespace SafeFlow.Domain.Incidents.Entities;

/// <summary>
/// Represents a corrective or preventive action assigned to remediate an Incident.
/// </summary>
public sealed class CorrectiveAction : BaseEntity
{
    private CorrectiveAction() { }

    public Guid IncidentId { get; private set; }
    public CorrectiveActionDescription Description { get; private set; } = default!;
    public Guid AssignedToEmployeeId { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? CompletedByEmployeeId { get; private set; }
    public CorrectiveActionStatus Status { get; private set; }

    internal static CorrectiveAction Create(
        Guid incidentId,
        CorrectiveActionDescription description,
        Guid assignedToEmployeeId,
        DateTime dueDate)
    {
        if (incidentId == Guid.Empty)
            throw new ArgumentException("IncidentId must not be empty.", nameof(incidentId));

        ArgumentNullException.ThrowIfNull(description, nameof(description));

        if (assignedToEmployeeId == Guid.Empty)
            throw new ArgumentException("AssignedToEmployeeId must not be empty.", nameof(assignedToEmployeeId));

        return new CorrectiveAction
        {
            Id = Guid.NewGuid(),
            IncidentId = incidentId,
            Description = description,
            AssignedToEmployeeId = assignedToEmployeeId,
            DueDate = dueDate,
            Status = CorrectiveActionStatus.Open
        };
    }

    internal void MarkAsCompleted(Guid completedByEmployeeId)
    {
        if (completedByEmployeeId == Guid.Empty)
            throw new ArgumentException("CompletedByEmployeeId must not be empty.", nameof(completedByEmployeeId));

        Status = CorrectiveActionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        CompletedByEmployeeId = completedByEmployeeId;
    }
}
