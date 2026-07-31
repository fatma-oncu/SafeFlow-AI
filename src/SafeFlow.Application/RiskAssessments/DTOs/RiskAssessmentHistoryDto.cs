using SafeFlow.Domain.RiskAssessments.Entities;

namespace SafeFlow.Application.RiskAssessments.DTOs;

/// <summary>
/// Data Transfer Object for lifecycle audit log entries.
/// </summary>
public sealed class RiskAssessmentHistoryDto
{
    public Guid Id { get; init; }
    public Guid RiskAssessmentId { get; init; }
    public string Action { get; init; } = default!;
    public Guid PerformedByEmployeeId { get; init; }
    public string? OldStatus { get; init; }
    public string NewStatus { get; init; } = default!;
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }

    public static RiskAssessmentHistoryDto FromEntity(RiskAssessmentHistory entity) => new()
    {
        Id = entity.Id,
        RiskAssessmentId = entity.RiskAssessmentId,
        Action = entity.Action.ToString(),
        PerformedByEmployeeId = entity.PerformedByEmployeeId,
        OldStatus = entity.OldStatus?.ToString(),
        NewStatus = entity.NewStatus.ToString(),
        Comment = entity.Comment,
        CreatedAt = entity.CreatedAt
    };
}
