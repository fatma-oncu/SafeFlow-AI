using SafeFlow.Domain.RiskAssessments.Aggregates;

namespace SafeFlow.Application.RiskAssessments.DTOs;

/// <summary>
/// Detailed Data Transfer Object for a <see cref="RiskAssessment"/> aggregate root.
/// </summary>
public sealed class RiskAssessmentDto
{
    public Guid Id { get; init; }
    public string AssessmentNumber { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public Guid DepartmentId { get; init; }
    public Guid CreatedByEmployeeId { get; init; }
    public Guid ResponsibleEmployeeId { get; init; }
    public Guid? ApprovedByEmployeeId { get; init; }
    public string Status { get; init; } = default!;
    public string OverallRiskLevel { get; init; } = default!;
    public int RevisionNumber { get; init; }
    public Guid? PreviousAssessmentId { get; init; }
    public DateTime? NextReviewDate { get; init; }
    public Guid TenantId { get; init; }
    public byte[] RowVersion { get; init; } = [];
    public DateTime CreatedAt { get; init; }

    public IReadOnlyList<RiskHazardDto> Hazards { get; init; } = [];
    public IReadOnlyList<RiskAssessmentApprovalDto> Approvals { get; init; } = [];
    public IReadOnlyList<RiskAssessmentHistoryDto> History { get; init; } = [];

    public static RiskAssessmentDto FromAggregate(RiskAssessment aggregate) => new()
    {
        Id = aggregate.Id,
        AssessmentNumber = aggregate.AssessmentNumber.Value,
        Title = aggregate.Title,
        Description = aggregate.Description,
        DepartmentId = aggregate.DepartmentId,
        CreatedByEmployeeId = aggregate.CreatedByEmployeeId,
        ResponsibleEmployeeId = aggregate.ResponsibleEmployeeId,
        ApprovedByEmployeeId = aggregate.ApprovedByEmployeeId,
        Status = aggregate.Status.ToString(),
        OverallRiskLevel = aggregate.OverallRiskLevel.ToString(),
        RevisionNumber = aggregate.RevisionNumber,
        PreviousAssessmentId = aggregate.PreviousAssessmentId,
        NextReviewDate = aggregate.NextReviewDate,
        TenantId = aggregate.TenantId,
        RowVersion = aggregate.RowVersion,
        CreatedAt = aggregate.CreatedAt,
        Hazards = aggregate.Hazards.Select(RiskHazardDto.FromEntity).ToList(),
        Approvals = aggregate.Approvals.Select(RiskAssessmentApprovalDto.FromEntity).ToList(),
        History = aggregate.History.Select(RiskAssessmentHistoryDto.FromEntity).ToList()
    };
}
