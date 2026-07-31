using SafeFlow.Domain.Incidents.Aggregates;

namespace SafeFlow.Application.Incidents.DTOs;

/// <summary>Detailed Data Transfer Object for an Incident aggregate root.</summary>
public sealed class IncidentDto
{
    public Guid Id { get; init; }
    public string IncidentNumber { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public string Location { get; init; } = default!;
    public DateTime OccurredAt { get; init; }
    public string Severity { get; init; } = default!;
    public string Category { get; init; } = default!;
    public string Status { get; init; } = default!;
    public string? InvestigationResult { get; init; }
    public string? ResolutionSummary { get; init; }
    public string? ClosureNotes { get; init; }
    public Guid DepartmentId { get; init; }
    public Guid ReportedByEmployeeId { get; init; }
    public Guid? AffectedEmployeeId { get; init; }
    public Guid? AssignedToEmployeeId { get; init; }
    public Guid? InvestigatedByEmployeeId { get; init; }
    public Guid? ClosedByEmployeeId { get; init; }
    public Guid? RiskAssessmentId { get; init; }
    public Guid TenantId { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTime CreatedAt { get; init; }

    public IReadOnlyList<IncidentAttachmentDto> Attachments { get; init; } = [];
    public IReadOnlyList<IncidentCommentDto> Comments { get; init; } = [];
    public IReadOnlyList<CorrectiveActionDto> CorrectiveActions { get; init; } = [];

    public static IncidentDto FromAggregate(Incident aggregate) => new()
    {
        Id = aggregate.Id,
        IncidentNumber = aggregate.IncidentNumber.Value,
        Title = aggregate.Title.Value,
        Description = aggregate.Description.Value,
        Location = aggregate.Location.Value,
        OccurredAt = aggregate.OccurredAt,
        Severity = aggregate.Severity.ToString(),
        Category = aggregate.Category.ToString(),
        Status = aggregate.Status.ToString(),
        InvestigationResult = aggregate.InvestigationResult?.ToString(),
        ResolutionSummary = aggregate.ResolutionSummary,
        ClosureNotes = aggregate.ClosureNotes,
        DepartmentId = aggregate.DepartmentId,
        ReportedByEmployeeId = aggregate.ReportedByEmployeeId,
        AffectedEmployeeId = aggregate.AffectedEmployeeId,
        AssignedToEmployeeId = aggregate.AssignedToEmployeeId,
        InvestigatedByEmployeeId = aggregate.InvestigatedByEmployeeId,
        ClosedByEmployeeId = aggregate.ClosedByEmployeeId,
        RiskAssessmentId = aggregate.RiskAssessmentId,
        TenantId = aggregate.TenantId,
        RowVersion = aggregate.RowVersion,
        CreatedAt = aggregate.CreatedAt,
        Attachments = aggregate.Attachments.Select(IncidentAttachmentDto.FromEntity).ToList(),
        Comments = aggregate.Comments.Select(IncidentCommentDto.FromEntity).ToList(),
        CorrectiveActions = aggregate.CorrectiveActions.Select(CorrectiveActionDto.FromEntity).ToList()
    };
}
