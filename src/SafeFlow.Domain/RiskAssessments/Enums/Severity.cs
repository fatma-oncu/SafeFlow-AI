namespace SafeFlow.Domain.RiskAssessments.Enums;

/// <summary>
/// Represents the severity rating of potential harm/consequence from a hazard.
/// </summary>
public enum Severity
{
    /// <summary>Negligible consequence (1).</summary>
    Negligible = 1,

    /// <summary>Minor consequence (2).</summary>
    Minor = 2,

    /// <summary>Moderate consequence (3).</summary>
    Moderate = 3,

    /// <summary>Major consequence (4).</summary>
    Major = 4,

    /// <summary>Critical / catastrophic consequence (5).</summary>
    Critical = 5
}
