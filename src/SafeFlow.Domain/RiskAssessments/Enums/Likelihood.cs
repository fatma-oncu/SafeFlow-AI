namespace SafeFlow.Domain.RiskAssessments.Enums;

/// <summary>
/// Represents the likelihood rating of a hazard occurring.
/// </summary>
public enum Likelihood
{
    /// <summary>Rare / highly improbable occurrence (1).</summary>
    Rare = 1,

    /// <summary>Unlikely occurrence (2).</summary>
    Unlikely = 2,

    /// <summary>Possible occurrence (3).</summary>
    Possible = 3,

    /// <summary>Likely occurrence (4).</summary>
    Likely = 4,

    /// <summary>Almost certain occurrence (5).</summary>
    AlmostCertain = 5
}
