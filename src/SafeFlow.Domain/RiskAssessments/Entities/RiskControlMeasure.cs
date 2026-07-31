using SafeFlow.Domain.RiskAssessments.Enums;
using SafeFlow.Domain.RiskAssessments.ValueObjects;
using SafeFlow.SharedKernel.Entities;

namespace SafeFlow.Domain.RiskAssessments.Entities;

/// <summary>
/// Represents a mitigation control measure applied to a specific <see cref="RiskHazard"/>.
/// </summary>
public sealed class RiskControlMeasure : BaseEntity
{
    private RiskControlMeasure() { }

    /// <summary>Gets the parent hazard identifier.</summary>
    public Guid RiskHazardId { get; private set; }

    /// <summary>Gets the control description value object.</summary>
    public ControlDescription Description { get; private set; } = default!;

    /// <summary>Gets the control measure classification type.</summary>
    public ControlMeasureType Type { get; private set; }

    /// <summary>Gets whether the control measure has been fully implemented.</summary>
    public bool IsImplemented { get; private set; }

    /// <summary>Gets the timestamp when the control measure was implemented.</summary>
    public DateTime? ImplementedAt { get; private set; }

    internal static RiskControlMeasure Create(
        Guid hazardId,
        ControlDescription description,
        ControlMeasureType type,
        bool isImplemented = false)
    {
        if (hazardId == Guid.Empty)
            throw new ArgumentException("HazardId must not be empty.", nameof(hazardId));
        ArgumentNullException.ThrowIfNull(description, nameof(description));

        return new RiskControlMeasure
        {
            Id = Guid.NewGuid(),
            RiskHazardId = hazardId,
            Description = description,
            Type = type,
            IsImplemented = isImplemented,
            ImplementedAt = isImplemented ? DateTime.UtcNow : null
        };
    }

    internal void Update(ControlDescription description, ControlMeasureType type, bool isImplemented)
    {
        ArgumentNullException.ThrowIfNull(description, nameof(description));

        Description = description;
        Type = type;
        if (!IsImplemented && isImplemented)
        {
            ImplementedAt = DateTime.UtcNow;
        }
        else if (!isImplemented)
        {
            ImplementedAt = null;
        }
        IsImplemented = isImplemented;
    }
}
