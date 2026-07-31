using SafeFlow.Domain.Incidents.Entities;
using SafeFlow.Domain.Incidents.Enums;
using SafeFlow.Domain.Incidents.Events;
using SafeFlow.Domain.Incidents.ValueObjects;
using SafeFlow.SharedKernel.Entities;
using SafeFlow.SharedKernel.Exceptions;

namespace SafeFlow.Domain.Incidents.Aggregates;

/// <summary>
/// Aggregate Root representing a workplace Incident in SafeFlow.
/// Manages incident classification, workflow status transitions, corrective actions, attachments, and comments.
/// </summary>
public sealed class Incident : AggregateRoot
{
    private readonly List<IncidentAttachment> _attachments = [];
    private readonly List<IncidentComment> _comments = [];
    private readonly List<CorrectiveAction> _correctiveActions = [];

    private Incident() { }

    // ── Properties ───────────────────────────────────────────────────────────

    /// <summary>Gets the formatted unique incident number (e.g. INC-2026-000001).</summary>
    public IncidentNumber IncidentNumber { get; private set; } = default!;

    /// <summary>Gets the incident title.</summary>
    public IncidentTitle Title { get; private set; } = default!;

    /// <summary>Gets the detailed incident description.</summary>
    public IncidentDescription Description { get; private set; } = default!;

    /// <summary>Gets the physical location of the incident.</summary>
    public IncidentLocation Location { get; private set; } = default!;

    /// <summary>Gets the date and time when the incident occurred.</summary>
    public DateTime OccurredAt { get; private set; }

    /// <summary>Gets the incident severity level.</summary>
    public IncidentSeverity Severity { get; private set; }

    /// <summary>Gets the incident category.</summary>
    public IncidentCategory Category { get; private set; }

    /// <summary>Gets the current workflow status of the incident.</summary>
    public IncidentStatus Status { get; private set; }

    /// <summary>Gets the identified root cause investigation result (if resolved).</summary>
    public InvestigationResult? InvestigationResult { get; private set; }

    /// <summary>Gets the investigation resolution summary notes.</summary>
    public string? ResolutionSummary { get; private set; }

    /// <summary>Gets the formal closure notes (if closed).</summary>
    public string? ClosureNotes { get; private set; }

    /// <summary>Gets the assigned department identifier.</summary>
    public Guid DepartmentId { get; private set; }

    /// <summary>Gets the Employee identifier who reported the incident.</summary>
    public Guid ReportedByEmployeeId { get; private set; }

    /// <summary>Gets the optional Employee identifier affected by the incident.</summary>
    public Guid? AffectedEmployeeId { get; private set; }

    /// <summary>Gets the Employee identifier assigned as responsible investigator.</summary>
    public Guid? AssignedToEmployeeId { get; private set; }

    /// <summary>Gets the Employee identifier who conducted the investigation.</summary>
    public Guid? InvestigatedByEmployeeId { get; private set; }

    /// <summary>Gets the Employee identifier who formally closed the incident.</summary>
    public Guid? ClosedByEmployeeId { get; private set; }

    /// <summary>Gets the optional linked Risk Assessment identifier.</summary>
    public Guid? RiskAssessmentId { get; private set; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Gets the concurrency token (row version).</summary>
    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    /// <summary>Gets the read-only collection of attachments.</summary>
    public IReadOnlyCollection<IncidentAttachment> Attachments => _attachments.AsReadOnly();

    /// <summary>Gets the read-only collection of comments.</summary>
    public IReadOnlyCollection<IncidentComment> Comments => _comments.AsReadOnly();

    /// <summary>Gets the read-only collection of corrective actions.</summary>
    public IReadOnlyCollection<CorrectiveAction> CorrectiveActions => _correctiveActions.AsReadOnly();

    // ── Factory Method ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="Incident"/> aggregate root in Reported status.
    /// </summary>
    public static Incident Create(
        IncidentNumber number,
        IncidentTitle title,
        IncidentDescription description,
        IncidentLocation location,
        IncidentSeverity severity,
        IncidentCategory category,
        DateTime occurredAt,
        Guid departmentId,
        Guid reportedByEmployeeId,
        Guid tenantId,
        Guid? riskAssessmentId = null,
        Guid? affectedEmployeeId = null)
    {
        ArgumentNullException.ThrowIfNull(number, nameof(number));
        ArgumentNullException.ThrowIfNull(title, nameof(title));
        ArgumentNullException.ThrowIfNull(description, nameof(description));
        ArgumentNullException.ThrowIfNull(location, nameof(location));

        if (departmentId == Guid.Empty)
            throw new ValidationException(nameof(departmentId), "DepartmentId must not be empty.");

        if (reportedByEmployeeId == Guid.Empty)
            throw new ValidationException(nameof(reportedByEmployeeId), "ReportedByEmployeeId must not be empty.");

        if (tenantId == Guid.Empty)
            throw new ValidationException(nameof(tenantId), "TenantId must not be empty.");

        var incident = new Incident
        {
            Id = Guid.NewGuid(),
            IncidentNumber = number,
            Title = title,
            Description = description,
            Location = location,
            Severity = severity,
            Category = category,
            OccurredAt = occurredAt,
            DepartmentId = departmentId,
            ReportedByEmployeeId = reportedByEmployeeId,
            AffectedEmployeeId = affectedEmployeeId,
            RiskAssessmentId = riskAssessmentId,
            Status = IncidentStatus.Reported,
            TenantId = tenantId
        };

        incident.RaiseDomainEvent(new IncidentReportedDomainEvent(incident));
        return incident;
    }

    // ── Aggregate Domain Methods ─────────────────────────────────────────────

    /// <summary>
    /// Updates incident header details.
    /// </summary>
    public void UpdateDetails(
        IncidentTitle title,
        IncidentDescription description,
        IncidentLocation location,
        IncidentSeverity severity,
        IncidentCategory category,
        DateTime occurredAt,
        Guid departmentId,
        Guid? riskAssessmentId,
        Guid? affectedEmployeeId)
    {
        EnsureNotClosedOrCancelled();

        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Severity = severity;
        Category = category;
        OccurredAt = occurredAt;
        DepartmentId = departmentId != Guid.Empty ? departmentId : throw new ArgumentException("DepartmentId required.", nameof(departmentId));
        RiskAssessmentId = riskAssessmentId;
        AffectedEmployeeId = affectedEmployeeId;
        RowVersion = Guid.NewGuid().ToByteArray();
    }

    /// <summary>
    /// Assigns the incident to a responsible investigator.
    /// </summary>
    public void Assign(Guid assignedToEmployeeId)
    {
        EnsureNotClosedOrCancelled();

        if (assignedToEmployeeId == Guid.Empty)
            throw new ValidationException(nameof(assignedToEmployeeId), "AssignedToEmployeeId must not be empty.");

        AssignedToEmployeeId = assignedToEmployeeId;
        Status = IncidentStatus.Assigned;
        RowVersion = Guid.NewGuid().ToByteArray();

        RaiseDomainEvent(new IncidentAssignedDomainEvent(this, assignedToEmployeeId));
    }

    /// <summary>
    /// Commences formal investigation on the incident.
    /// </summary>
    public void StartInvestigation(Guid investigatorEmployeeId)
    {
        EnsureNotClosedOrCancelled();

        if (investigatorEmployeeId == Guid.Empty)
            throw new ValidationException(nameof(investigatorEmployeeId), "InvestigatorEmployeeId must not be empty.");

        InvestigatedByEmployeeId = investigatorEmployeeId;
        if (AssignedToEmployeeId is null)
        {
            AssignedToEmployeeId = investigatorEmployeeId;
        }

        Status = IncidentStatus.UnderInvestigation;
        RowVersion = Guid.NewGuid().ToByteArray();

        RaiseDomainEvent(new IncidentInvestigationStartedDomainEvent(this, investigatorEmployeeId));
    }

    /// <summary>
    /// Adds a comment/note to the incident log.
    /// </summary>
    public IncidentComment AddComment(Guid authorEmployeeId, string content)
    {
        EnsureNotClosedOrCancelled();

        var comment = IncidentComment.Create(Id, authorEmployeeId, content);
        _comments.Add(comment);
        RowVersion = Guid.NewGuid().ToByteArray();

        RaiseDomainEvent(new CommentAddedDomainEvent(this, comment));
        return comment;
    }

    /// <summary>
    /// Adds an attachment file to the incident.
    /// </summary>
    public IncidentAttachment AddAttachment(
        string fileName,
        string fileUrl,
        string contentType,
        long sizeBytes,
        Guid uploadedByEmployeeId)
    {
        EnsureNotClosedOrCancelled();

        var attachment = IncidentAttachment.Create(Id, fileName, fileUrl, contentType, sizeBytes, uploadedByEmployeeId);
        _attachments.Add(attachment);
        RowVersion = Guid.NewGuid().ToByteArray();

        return attachment;
    }

    /// <summary>
    /// Adds a corrective action to remediate the incident.
    /// </summary>
    public CorrectiveAction AddCorrectiveAction(
        CorrectiveActionDescription description,
        Guid assignedToEmployeeId,
        DateTime dueDate)
    {
        EnsureNotClosedOrCancelled();

        var action = CorrectiveAction.Create(Id, description, assignedToEmployeeId, dueDate);
        _correctiveActions.Add(action);

        if (Status == IncidentStatus.UnderInvestigation)
        {
            Status = IncidentStatus.WaitingCorrectiveAction;
        }

        RowVersion = Guid.NewGuid().ToByteArray();

        RaiseDomainEvent(new CorrectiveActionAddedDomainEvent(this, action));
        return action;
    }

    /// <summary>
    /// Marks a corrective action as completed.
    /// </summary>
    public void CompleteCorrectiveAction(Guid actionId, Guid completedByEmployeeId)
    {
        EnsureNotClosedOrCancelled();

        var action = _correctiveActions.FirstOrDefault(a => a.Id == actionId);
        if (action is null)
            throw new ValidationException(nameof(actionId), $"Corrective action with ID '{actionId}' not found.");

        action.MarkAsCompleted(completedByEmployeeId);
        RowVersion = Guid.NewGuid().ToByteArray();

        RaiseDomainEvent(new CorrectiveActionCompletedDomainEvent(this, action));
    }

    /// <summary>
    /// Resolves the incident with investigation findings.
    /// </summary>
    public void Resolve(InvestigationResult investigationResult, string resolutionSummary)
    {
        EnsureNotClosedOrCancelled();

        if (string.IsNullOrWhiteSpace(resolutionSummary))
            throw new ValidationException(nameof(resolutionSummary), "Resolution summary is required.");

        // Check if all corrective actions are completed
        bool uncompletedActions = _correctiveActions.Any(a => a.Status != CorrectiveActionStatus.Completed && a.Status != CorrectiveActionStatus.Verified);
        if (uncompletedActions)
            throw new ValidationException(nameof(CorrectiveActions), "All corrective actions must be completed before resolving the incident.");

        InvestigationResult = investigationResult;
        ResolutionSummary = resolutionSummary.Trim();
        Status = IncidentStatus.Resolved;
        RowVersion = Guid.NewGuid().ToByteArray();

        RaiseDomainEvent(new IncidentResolvedDomainEvent(this, investigationResult));
    }

    /// <summary>
    /// Formally closes the incident.
    /// </summary>
    public void Close(Guid closedByEmployeeId, string? closureNotes)
    {
        if (Status != IncidentStatus.Resolved)
            throw new ValidationException(nameof(Status), $"Cannot close incident in {Status} status. Must be Resolved.");

        if (closedByEmployeeId == Guid.Empty)
            throw new ValidationException(nameof(closedByEmployeeId), "ClosedByEmployeeId must not be empty.");

        ClosedByEmployeeId = closedByEmployeeId;
        ClosureNotes = closureNotes?.Trim();
        Status = IncidentStatus.Closed;
        RowVersion = Guid.NewGuid().ToByteArray();

        RaiseDomainEvent(new IncidentClosedDomainEvent(this, closedByEmployeeId));
    }

    /// <summary>
    /// Cancels the incident report.
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == IncidentStatus.Closed || Status == IncidentStatus.Cancelled)
            throw new ValidationException(nameof(Status), $"Cannot cancel incident in {Status} status.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ValidationException(nameof(reason), "Cancellation reason is required.");

        Status = IncidentStatus.Cancelled;
        ClosureNotes = $"Cancelled: {reason.Trim()}";
        RowVersion = Guid.NewGuid().ToByteArray();

        RaiseDomainEvent(new IncidentCancelledDomainEvent(this, reason));
    }

    /// <summary>
    /// Reopens a closed or cancelled incident for further investigation.
    /// </summary>
    public void Reopen(string reason)
    {
        if (Status != IncidentStatus.Closed && Status != IncidentStatus.Cancelled)
            throw new ValidationException(nameof(Status), $"Cannot reopen incident in {Status} status. Must be Closed or Cancelled.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ValidationException(nameof(reason), "Reopen reason is required.");

        Status = IncidentStatus.UnderInvestigation;
        RowVersion = Guid.NewGuid().ToByteArray();

        RaiseDomainEvent(new IncidentReopenedDomainEvent(this, reason));
    }

    /// <summary>
    /// Soft-deletes the incident aggregate.
    /// </summary>
    public void SoftDelete(string? deletedBy)
    {
        if (IsDeleted) return;

        base.SoftDelete(deletedBy ?? "System");
        RowVersion = Guid.NewGuid().ToByteArray();
    }

    private void EnsureNotClosedOrCancelled()
    {
        if (Status == IncidentStatus.Closed || Status == IncidentStatus.Cancelled)
        {
            throw new ValidationException(nameof(Status), $"Incident in {Status} status cannot be modified.");
        }
    }
}
