using SafeFlow.Domain.Incidents.Enums;

namespace SafeFlow.Application.Incidents.DTOs;

/// <summary>HTTP API request contract models for Incident endpoints.</summary>
public static class IncidentRequests
{
    public sealed record CreateIncidentRequest(
        string Title,
        string Description,
        string Location,
        IncidentSeverity Severity,
        IncidentCategory Category,
        DateTime OccurredAt,
        Guid DepartmentId,
        Guid ReportedByEmployeeId,
        Guid TenantId,
        Guid? RiskAssessmentId = null,
        Guid? AffectedEmployeeId = null);

    public sealed record UpdateIncidentRequest(
        string Title,
        string Description,
        string Location,
        IncidentSeverity Severity,
        IncidentCategory Category,
        DateTime OccurredAt,
        Guid DepartmentId,
        Guid? RiskAssessmentId,
        Guid? AffectedEmployeeId,
        byte[] RowVersion);

    public sealed record AssignIncidentRequest(
        Guid AssignedToEmployeeId);

    public sealed record StartInvestigationRequest(
        Guid InvestigatorEmployeeId);

    public sealed record AddCommentRequest(
        Guid AuthorEmployeeId,
        string Content);

    public sealed record AddAttachmentRequest(
        string FileName,
        string FileUrl,
        string ContentType,
        long SizeBytes,
        Guid UploadedByEmployeeId);

    public sealed record AddCorrectiveActionRequest(
        string Description,
        Guid AssignedToEmployeeId,
        DateTime DueDate);

    public sealed record CompleteCorrectiveActionRequest(
        Guid CompletedByEmployeeId);

    public sealed record ResolveIncidentRequest(
        InvestigationResult InvestigationResult,
        string ResolutionSummary);

    public sealed record CloseIncidentRequest(
        Guid ClosedByEmployeeId,
        string? ClosureNotes);

    public sealed record CancelIncidentRequest(
        string Reason);

    public sealed record ReopenIncidentRequest(
        string Reason);
}
