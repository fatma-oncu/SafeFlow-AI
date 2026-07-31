using SafeFlow.Domain.RiskAssessments.Entities;

namespace SafeFlow.Application.RiskAssessments.DTOs;

/// <summary>
/// Data Transfer Object for audit trail approval decisions.
/// </summary>
public sealed class RiskAssessmentApprovalDto
{
    public Guid Id { get; init; }
    public Guid RiskAssessmentId { get; init; }
    public Guid EmployeeId { get; init; }
    public string Decision { get; init; } = default!;
    public string? Comment { get; init; }
    public DateTime ProcessedAt { get; init; }

    public static RiskAssessmentApprovalDto FromEntity(RiskAssessmentApproval entity) => new()
    {
        Id = entity.Id,
        RiskAssessmentId = entity.RiskAssessmentId,
        EmployeeId = entity.EmployeeId,
        Decision = entity.Decision.ToString(),
        Comment = entity.Comment,
        ProcessedAt = entity.ProcessedAt
    };
}
