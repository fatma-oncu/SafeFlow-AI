using SafeFlow.Domain.RiskAssessments.Aggregates;
using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.Domain.RiskAssessments.ValueObjects;
using SafeFlow.SharedKernel.Entities;

namespace SafeFlow.Domain.RiskAssessments.Entities;

/// <summary>
/// Represents a hazard identified within a <see cref="RiskAssessment"/>.
/// </summary>
public sealed class RiskHazard : BaseEntity
{
    private readonly List<RiskControlMeasure> _controlMeasures = [];

    private RiskHazard() { }

    /// <summary>Gets the parent risk assessment identifier.</summary>
    public Guid RiskAssessmentId { get; private set; }

    /// <summary>Gets the hazard description value object.</summary>
    public HazardDescription Description { get; private set; } = default!;

    /// <summary>Gets the initial risk score before controls.</summary>
    public RiskMatrixScore InitialScore { get; private set; } = default!;

    /// <summary>Gets the residual risk score after controls are applied.</summary>
    public RiskMatrixScore ResidualScore { get; private set; } = default!;

    /// <summary>Gets the read-only collection of mitigation control measures.</summary>
    public IReadOnlyCollection<RiskControlMeasure> ControlMeasures => _controlMeasures.AsReadOnly();

    internal static RiskHazard Create(
        Guid assessmentId,
        HazardDescription description,
        RiskMatrixScore initialScore,
        RiskMatrixScore residualScore)
    {
        if (assessmentId == Guid.Empty)
            throw new ArgumentException("RiskAssessmentId must not be empty.", nameof(assessmentId));

        ArgumentNullException.ThrowIfNull(description, nameof(description));
        ArgumentNullException.ThrowIfNull(initialScore, nameof(initialScore));
        ArgumentNullException.ThrowIfNull(residualScore, nameof(residualScore));

        return new RiskHazard
        {
            Id = Guid.NewGuid(),
            RiskAssessmentId = assessmentId,
            Description = description,
            InitialScore = initialScore,
            ResidualScore = residualScore
        };
    }

    internal void UpdateScores(
        HazardDescription description,
        RiskMatrixScore initialScore,
        RiskMatrixScore residualScore)
    {
        ArgumentNullException.ThrowIfNull(description, nameof(description));
        ArgumentNullException.ThrowIfNull(initialScore, nameof(initialScore));
        ArgumentNullException.ThrowIfNull(residualScore, nameof(residualScore));

        Description = description;
        InitialScore = initialScore;
        ResidualScore = residualScore;
    }

    internal RiskControlMeasure AddControlMeasure(
        ControlDescription description,
        ControlMeasureType type,
        bool isImplemented = false)
    {
        var control = RiskControlMeasure.Create(Id, description, type, isImplemented);
        _controlMeasures.Add(control);
        return control;
    }

    internal bool RemoveControlMeasure(Guid controlId)
    {
        int removed = _controlMeasures.RemoveAll(c => c.Id == controlId);
        return removed > 0;
    }
}
