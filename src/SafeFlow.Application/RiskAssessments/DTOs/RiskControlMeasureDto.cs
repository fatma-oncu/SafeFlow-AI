using SafeFlow.Domain.RiskAssessments.Entities;

namespace SafeFlow.Application.RiskAssessments.DTOs;

/// <summary>
/// Data Transfer Object for a mitigation control measure.
/// </summary>
public sealed class RiskControlMeasureDto
{
    public Guid Id { get; init; }
    public Guid RiskHazardId { get; init; }
    public string Description { get; init; } = default!;
    public string Type { get; init; } = default!;
    public bool IsImplemented { get; init; }
    public DateTime? ImplementedAt { get; init; }

    public static RiskControlMeasureDto FromEntity(RiskControlMeasure entity) => new()
    {
        Id = entity.Id,
        RiskHazardId = entity.RiskHazardId,
        Description = entity.Description.Value,
        Type = entity.Type.ToString(),
        IsImplemented = entity.IsImplemented,
        ImplementedAt = entity.ImplementedAt
    };
}
