namespace SafeFlow.Domain.RiskAssessments.Enums;

/// <summary>
/// Represents the overall risk evaluation level derived from the Risk Matrix.
/// </summary>
public enum RiskLevel
{
    /// <summary>Low risk (Score 1-4).</summary>
    Low = 1,

    /// <summary>Medium risk (Score 5-9).</summary>
    Medium = 2,

    /// <summary>High risk (Score 10-15).</summary>
    High = 3,

    /// <summary>Critical risk (Score 16-25).</summary>
    Critical = 4
}
