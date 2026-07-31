using SafeFlow.Domain.RiskAssessments.Entities;

namespace SafeFlow.Application.RiskAssessments.DTOs;

/// <summary>
/// Data Transfer Object for a hazard within a risk assessment.
/// </summary>
public sealed class RiskHazardDto
{
    public Guid Id { get; init; }
    public Guid RiskAssessmentId { get; init; }
    public string Description { get; init; } = default!;

    public int InitialLikelihood { get; init; }
    public int InitialSeverity { get; init; }
    public int InitialScore { get; init; }
    public string InitialRiskLevel { get; init; } = default!;

    public int ResidualLikelihood { get; init; }
    public int ResidualSeverity { get; init; }
    public int ResidualScore { get; init; }
    public string ResidualRiskLevel { get; init; } = default!;

    public IReadOnlyList<RiskControlMeasureDto> ControlMeasures { get; init; } = [];

    public static RiskHazardDto FromEntity(RiskHazard entity) => new()
    {
        Id = entity.Id,
        RiskAssessmentId = entity.RiskAssessmentId,
        Description = entity.Description.Value,
        InitialLikelihood = (int)entity.InitialScore.Likelihood,
        InitialSeverity = (int)entity.InitialScore.Severity,
        InitialScore = entity.InitialScore.Score,
        InitialRiskLevel = entity.InitialScore.RiskLevel.ToString(),
        ResidualLikelihood = (int)entity.ResidualScore.Likelihood,
        ResidualSeverity = (int)entity.ResidualScore.Severity,
        ResidualScore = entity.ResidualScore.Score,
        ResidualRiskLevel = entity.ResidualScore.RiskLevel.ToString(),
        ControlMeasures = entity.ControlMeasures.Select(RiskControlMeasureDto.FromEntity).ToList()
    };
}
