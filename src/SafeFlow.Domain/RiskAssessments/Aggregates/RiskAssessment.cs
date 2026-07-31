using SafeFlow.Domain.RiskAssessments.Entities;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.Domain.RiskAssessments.Events;
using SafeFlow.Domain.RiskAssessments.ValueObjects;
using SafeFlow.SharedKernel.Entities;
using SafeFlow.SharedKernel.Exceptions;

namespace SafeFlow.Domain.RiskAssessments.Aggregates;

/// <summary>
/// Aggregate Root representing an enterprise Risk Assessment within SafeFlow.
/// Manages hazards, controls, approvals, history, risk levels, and versioning.
/// </summary>
public sealed class RiskAssessment : AggregateRoot
{
    private readonly List<RiskHazard> _hazards = [];
    private readonly List<RiskAssessmentApproval> _approvals = [];
    private readonly List<RiskAssessmentHistory> _history = [];

    private RiskAssessment() { }

    // ── Properties ───────────────────────────────────────────────────────────

    /// <summary>Gets the unique formatted assessment number (e.g. RA-2026-000001).</summary>
    public RiskAssessmentNumber AssessmentNumber { get; private set; } = default!;

    /// <summary>Gets the assessment title.</summary>
    public string Title { get; private set; } = default!;

    /// <summary>Gets the assessment description.</summary>
    public string Description { get; private set; } = default!;

    /// <summary>Gets the assigned department identifier.</summary>
    public Guid DepartmentId { get; private set; }

    /// <summary>Gets the identifier of the Employee who created the assessment.</summary>
    public Guid CreatedByEmployeeId { get; private set; }

    /// <summary>Gets the identifier of the Employee responsible for the assessment.</summary>
    public Guid ResponsibleEmployeeId { get; private set; }

    /// <summary>Gets the identifier of the Employee who approved the assessment (if approved).</summary>
    public Guid? ApprovedByEmployeeId { get; private set; }

    /// <summary>Gets the current assessment lifecycle status.</summary>
    public AssessmentStatus Status { get; private set; }

    /// <summary>Gets the overall calculated risk level across all hazards.</summary>
    public RiskLevel OverallRiskLevel { get; private set; }

    /// <summary>Gets the revision version number (1, 2, 3...).</summary>
    public int RevisionNumber { get; private set; }

    /// <summary>Gets the optional identifier of the previous revision assessment.</summary>
    public Guid? PreviousAssessmentId { get; private set; }

    /// <summary>Gets the scheduled date for next review.</summary>
    public DateTime? NextReviewDate { get; private set; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Gets the concurrency token (row version).</summary>
    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    /// <summary>Gets the read-only collection of hazards.</summary>
    public IReadOnlyCollection<RiskHazard> Hazards => _hazards.AsReadOnly();

    /// <summary>Gets the read-only collection of approval history entries.</summary>
    public IReadOnlyCollection<RiskAssessmentApproval> Approvals => _approvals.AsReadOnly();

    /// <summary>Gets the read-only collection of history audit log entries.</summary>
    public IReadOnlyCollection<RiskAssessmentHistory> History => _history.AsReadOnly();

    // ── Factory Methods ──────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="RiskAssessment"/> in Draft status.
    /// </summary>
    public static RiskAssessment Create(
        RiskAssessmentNumber number,
        string title,
        string description,
        Guid departmentId,
        Guid createdByEmployeeId,
        Guid responsibleEmployeeId,
        Guid tenantId,
        DateTime? nextReviewDate = null)
    {
        ArgumentNullException.ThrowIfNull(number, nameof(number));

        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(title)] = ["Title must not be empty."]
            });

        if (departmentId == Guid.Empty)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(departmentId)] = ["DepartmentId must not be empty."]
            });

        if (createdByEmployeeId == Guid.Empty)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(createdByEmployeeId)] = ["CreatedByEmployeeId must not be empty."]
            });

        if (responsibleEmployeeId == Guid.Empty)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(responsibleEmployeeId)] = ["ResponsibleEmployeeId must not be empty."]
            });

        if (tenantId == Guid.Empty)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(tenantId)] = ["TenantId must not be empty."]
            });

        var assessment = new RiskAssessment
        {
            Id = Guid.NewGuid(),
            AssessmentNumber = number,
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            DepartmentId = departmentId,
            CreatedByEmployeeId = createdByEmployeeId,
            ResponsibleEmployeeId = responsibleEmployeeId,
            Status = AssessmentStatus.Draft,
            OverallRiskLevel = RiskLevel.Low,
            RevisionNumber = 1,
            PreviousAssessmentId = null,
            NextReviewDate = nextReviewDate,
            TenantId = tenantId
        };

        assessment._history.Add(RiskAssessmentHistory.Create(
            assessment.Id,
            RiskAssessmentAction.Created,
            createdByEmployeeId,
            null,
            AssessmentStatus.Draft,
            "Initial creation"));

        assessment.RaiseDomainEvent(new RiskAssessmentCreatedDomainEvent(assessment));
        return assessment;
    }

    /// <summary>
    /// Creates a new revision of an existing assessment.
    /// </summary>
    public RiskAssessment CreateRevision(
        RiskAssessmentNumber newNumber,
        Guid createdByEmployeeId)
    {
        EnsureNotArchived();
        ArgumentNullException.ThrowIfNull(newNumber, nameof(newNumber));

        var revision = new RiskAssessment
        {
            Id = Guid.NewGuid(),
            AssessmentNumber = newNumber,
            Title = Title,
            Description = Description,
            DepartmentId = DepartmentId,
            CreatedByEmployeeId = createdByEmployeeId,
            ResponsibleEmployeeId = ResponsibleEmployeeId,
            Status = AssessmentStatus.Draft,
            OverallRiskLevel = OverallRiskLevel,
            RevisionNumber = RevisionNumber + 1,
            PreviousAssessmentId = Id,
            NextReviewDate = NextReviewDate,
            TenantId = TenantId
        };

        // Copy hazards and controls to new revision
        foreach (var hazard in _hazards)
        {
            var newHazard = RiskHazard.Create(
                revision.Id,
                hazard.Description,
                hazard.InitialScore,
                hazard.ResidualScore);

            foreach (var control in hazard.ControlMeasures)
            {
                newHazard.AddControlMeasure(control.Description, control.Type, control.IsImplemented);
            }

            revision._hazards.Add(newHazard);
        }

        revision.RecalculateOverallRisk();

        revision._history.Add(RiskAssessmentHistory.Create(
            revision.Id,
            RiskAssessmentAction.RevisionCreated,
            createdByEmployeeId,
            null,
            AssessmentStatus.Draft,
            $"Created revision {revision.RevisionNumber} from assessment {Id}"));

        revision.RaiseDomainEvent(new RiskAssessmentCreatedDomainEvent(revision));
        return revision;
    }

    // ── Domain Actions ───────────────────────────────────────────────────────

    /// <summary>
    /// Updates assessment header details.
    /// </summary>
    public void UpdateDetails(
        string title,
        string description,
        Guid departmentId,
        Guid responsibleEmployeeId,
        DateTime? nextReviewDate)
    {
        EnsureNotArchived();

        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(title)] = ["Title must not be empty."]
            });

        if (departmentId == Guid.Empty)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(departmentId)] = ["DepartmentId must not be empty."]
            });

        if (responsibleEmployeeId == Guid.Empty)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(responsibleEmployeeId)] = ["ResponsibleEmployeeId must not be empty."]
            });

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        DepartmentId = departmentId;
        ResponsibleEmployeeId = responsibleEmployeeId;
        NextReviewDate = nextReviewDate;
        RowVersion = Guid.NewGuid().ToByteArray();

        _history.Add(RiskAssessmentHistory.Create(
            Id,
            RiskAssessmentAction.DetailsUpdated,
            responsibleEmployeeId,
            Status,
            Status,
            "Updated assessment header details"));

        RaiseDomainEvent(new RiskAssessmentUpdatedDomainEvent(this));
    }

    /// <summary>
    /// Submits assessment for approval review.
    /// </summary>
    public void SubmitForReview(Guid employeeId)
    {
        EnsureNotArchived();

        if (Status != AssessmentStatus.Draft && Status != AssessmentStatus.Rejected)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(Status)] = [$"Cannot submit assessment in {Status} status for review."]
            });

        if (_hazards.Count == 0)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(Hazards)] = ["Assessment must contain at least one hazard before submitting for review."]
            });

        var oldStatus = Status;
        Status = AssessmentStatus.InReview;
        RowVersion = Guid.NewGuid().ToByteArray();

        _approvals.Add(RiskAssessmentApproval.Create(Id, employeeId, ApprovalDecision.Submitted, "Submitted for review"));
        _history.Add(RiskAssessmentHistory.Create(Id, RiskAssessmentAction.SubmittedForReview, employeeId, oldStatus, Status, "Submitted for review"));

        RaiseDomainEvent(new RiskAssessmentSubmittedForReviewDomainEvent(this, employeeId));
    }

    /// <summary>
    /// Approves assessment.
    /// </summary>
    public void Approve(Guid approverEmployeeId, string? comment)
    {
        EnsureNotArchived();

        if (Status != AssessmentStatus.InReview)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(Status)] = [$"Cannot approve assessment in {Status} status. Must be InReview."]
            });

        if (approverEmployeeId == Guid.Empty)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(approverEmployeeId)] = ["ApproverEmployeeId must not be empty."]
            });

        var oldStatus = Status;
        Status = AssessmentStatus.Approved;
        ApprovedByEmployeeId = approverEmployeeId;
        RowVersion = Guid.NewGuid().ToByteArray();

        _approvals.Add(RiskAssessmentApproval.Create(Id, approverEmployeeId, ApprovalDecision.Approved, comment));
        _history.Add(RiskAssessmentHistory.Create(Id, RiskAssessmentAction.Approved, approverEmployeeId, oldStatus, Status, comment));

        RaiseDomainEvent(new RiskAssessmentApprovedDomainEvent(this, approverEmployeeId, comment));
    }

    /// <summary>
    /// Rejects assessment, returning it to draft state for corrections.
    /// </summary>
    public void Reject(Guid reviewerEmployeeId, string comment)
    {
        EnsureNotArchived();

        if (Status != AssessmentStatus.InReview)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(Status)] = [$"Cannot reject assessment in {Status} status. Must be InReview."]
            });

        if (string.IsNullOrWhiteSpace(comment))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(comment)] = ["Rejection comment is required."]
            });

        var oldStatus = Status;
        Status = AssessmentStatus.Rejected;
        RowVersion = Guid.NewGuid().ToByteArray();

        _approvals.Add(RiskAssessmentApproval.Create(Id, reviewerEmployeeId, ApprovalDecision.Rejected, comment));
        _history.Add(RiskAssessmentHistory.Create(Id, RiskAssessmentAction.Rejected, reviewerEmployeeId, oldStatus, Status, comment));

        RaiseDomainEvent(new RiskAssessmentRejectedDomainEvent(this, reviewerEmployeeId, comment));
    }

    /// <summary>
    /// Archives assessment.
    /// </summary>
    public void Archive()
    {
        if (Status == AssessmentStatus.Archived) return;

        var oldStatus = Status;
        Status = AssessmentStatus.Archived;
        RowVersion = Guid.NewGuid().ToByteArray();

        _history.Add(RiskAssessmentHistory.Create(Id, RiskAssessmentAction.Archived, ResponsibleEmployeeId, oldStatus, Status, "Archived risk assessment"));

        RaiseDomainEvent(new RiskAssessmentArchivedDomainEvent(this));
    }

    /// <summary>
    /// Adds a hazard to the assessment and recalculates overall risk level.
    /// </summary>
    public RiskHazard AddHazard(
        HazardDescription description,
        RiskMatrixScore initialScore,
        RiskMatrixScore residualScore)
    {
        EnsureNotArchived();

        var hazard = RiskHazard.Create(Id, description, initialScore, residualScore);
        _hazards.Add(hazard);

        RecalculateOverallRisk();
        RowVersion = Guid.NewGuid().ToByteArray();

        _history.Add(RiskAssessmentHistory.Create(Id, RiskAssessmentAction.HazardAdded, ResponsibleEmployeeId, Status, Status, $"Added hazard: {description.Value}"));

        RaiseDomainEvent(new HazardAddedDomainEvent(this, hazard));
        return hazard;
    }

    /// <summary>
    /// Updates a hazard's details and scores.
    /// </summary>
    public void UpdateHazard(
        Guid hazardId,
        HazardDescription description,
        RiskMatrixScore initialScore,
        RiskMatrixScore residualScore)
    {
        EnsureNotArchived();

        var hazard = _hazards.FirstOrDefault(h => h.Id == hazardId);
        if (hazard is null)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(hazardId)] = [$"Hazard with ID '{hazardId}' was not found in this assessment."]
            });

        hazard.UpdateScores(description, initialScore, residualScore);
        RecalculateOverallRisk();
        RowVersion = Guid.NewGuid().ToByteArray();

        _history.Add(RiskAssessmentHistory.Create(Id, RiskAssessmentAction.HazardUpdated, ResponsibleEmployeeId, Status, Status, $"Updated hazard: {description.Value}"));

        RaiseDomainEvent(new RiskAssessmentUpdatedDomainEvent(this));
    }

    /// <summary>
    /// Removes a hazard from the assessment.
    /// </summary>
    public void RemoveHazard(Guid hazardId)
    {
        EnsureNotArchived();

        int removed = _hazards.RemoveAll(h => h.Id == hazardId);
        if (removed > 0)
        {
            RecalculateOverallRisk();
            RowVersion = Guid.NewGuid().ToByteArray();

            _history.Add(RiskAssessmentHistory.Create(Id, RiskAssessmentAction.HazardRemoved, ResponsibleEmployeeId, Status, Status, $"Removed hazard ID: {hazardId}"));

            RaiseDomainEvent(new HazardRemovedDomainEvent(this, hazardId));
        }
    }

    /// <summary>
    /// Adds a control measure to a hazard.
    /// </summary>
    public RiskControlMeasure AddControlMeasure(
        Guid hazardId,
        ControlDescription description,
        ControlMeasureType type,
        bool isImplemented = false)
    {
        EnsureNotArchived();

        var hazard = _hazards.FirstOrDefault(h => h.Id == hazardId);
        if (hazard is null)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(hazardId)] = [$"Hazard with ID '{hazardId}' was not found in this assessment."]
            });

        var control = hazard.AddControlMeasure(description, type, isImplemented);
        RowVersion = Guid.NewGuid().ToByteArray();

        _history.Add(RiskAssessmentHistory.Create(Id, RiskAssessmentAction.ControlMeasureAdded, ResponsibleEmployeeId, Status, Status, $"Added control: {description.Value}"));

        RaiseDomainEvent(new ControlMeasureAddedDomainEvent(this, hazardId, control));
        return control;
    }

    /// <summary>
    /// Removes a control measure from a hazard.
    /// </summary>
    public void RemoveControlMeasure(Guid hazardId, Guid controlId)
    {
        EnsureNotArchived();

        var hazard = _hazards.FirstOrDefault(h => h.Id == hazardId);
        if (hazard is null) return;

        if (hazard.RemoveControlMeasure(controlId))
        {
            RowVersion = Guid.NewGuid().ToByteArray();
            _history.Add(RiskAssessmentHistory.Create(Id, RiskAssessmentAction.ControlMeasureRemoved, ResponsibleEmployeeId, Status, Status, $"Removed control ID: {controlId}"));

            RaiseDomainEvent(new ControlMeasureRemovedDomainEvent(this, hazardId, controlId));
        }
    }

    /// <summary>
    /// Recalculates the overall risk level derived from highest residual risk score across all hazards.
    /// </summary>
    public void RecalculateOverallRisk()
    {
        var previousLevel = OverallRiskLevel;

        if (_hazards.Count == 0)
        {
            OverallRiskLevel = RiskLevel.Low;
        }
        else
        {
            // Derived from highest residual score
            var highestResidual = _hazards.MaxBy(h => h.ResidualScore.Score)!.ResidualScore;
            OverallRiskLevel = highestResidual.RiskLevel;
        }

        if (previousLevel != OverallRiskLevel)
        {
            RaiseDomainEvent(new RiskLevelChangedDomainEvent(this, previousLevel, OverallRiskLevel));
        }
    }

    /// <summary>
    /// Soft deletes the risk assessment.
    /// </summary>
    public void SoftDelete(string? deletedBy)
    {
        if (IsDeleted) return;

        base.SoftDelete(deletedBy ?? "System");
        RowVersion = Guid.NewGuid().ToByteArray();

        _history.Add(RiskAssessmentHistory.Create(Id, RiskAssessmentAction.SoftDeleted, ResponsibleEmployeeId, Status, Status, "Soft deleted assessment"));

        RaiseDomainEvent(new RiskAssessmentSoftDeletedDomainEvent(this));
    }

    private void EnsureNotArchived()
    {
        if (Status == AssessmentStatus.Archived)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(Status)] = ["Archived risk assessments cannot be modified."]
            });
        }
    }
}
