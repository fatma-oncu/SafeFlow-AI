namespace SafeFlow.Domain.RiskAssessments.Enums;

/// <summary>
/// Hierarchy of controls for risk mitigation.
/// </summary>
public enum ControlMeasureType
{
    /// <summary>Eliminate the hazard completely (1).</summary>
    Elimination = 1,

    /// <summary>Substitute the hazard with a safer alternative (2).</summary>
    Substitution = 2,

    /// <summary>Isolate people from the hazard via engineering controls (3).</summary>
    Engineering = 3,

    /// <summary>Change the way people work via administrative procedures (4).</summary>
    Administrative = 4,

    /// <summary>Protect the worker with Personal Protective Equipment (5).</summary>
    PPE = 5
}
