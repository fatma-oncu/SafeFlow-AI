using SafeFlow.Domain.RiskAssessments.Aggregates;

namespace SafeFlow.Application.RiskAssessments.DTOs;

/// <summary>
/// Lightweight Data Transfer Object for risk assessment paged search results.
/// </summary>
public sealed class RiskAssessmentSearchResultDto
{
    public Guid Id { get; init; }
    public string AssessmentNumber { get; init; } = default!;
    public string Title { get; init; } = default!;
    public Guid DepartmentId { get; init; }
    public Guid ResponsibleEmployeeId { get; init; }
    public string Status { get; init; } = default!;
    public string OverallRiskLevel { get; init; } = default!;
    public int RevisionNumber { get; init; }
    public int HazardCount { get; init; }
    public DateTime CreatedAt { get; init; }

    public static RiskAssessmentSearchResultDto FromAggregate(RiskAssessment aggregate) => new()
    {
        Id = aggregate.Id,
        AssessmentNumber = aggregate.AssessmentNumber.Value,
        Title = aggregate.Title,
        DepartmentId = aggregate.DepartmentId,
        ResponsibleEmployeeId = aggregate.ResponsibleEmployeeId,
        Status = aggregate.Status.ToString(),
        OverallRiskLevel = aggregate.OverallRiskLevel.ToString(),
        RevisionNumber = aggregate.RevisionNumber,
        HazardCount = aggregate.Hazards.Count,
        CreatedAt = aggregate.CreatedAt
    };
}
