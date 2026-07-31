namespace SafeFlow.Domain.Incidents.Enums;

/// <summary>
/// Defines categories for workplace incidents.
/// </summary>
public enum IncidentCategory
{
    /// <summary>Near miss event without injury or damage.</summary>
    NearMiss = 1,

    /// <summary>First aid treatment required.</summary>
    FirstAid = 2,

    /// <summary>Medical treatment required by licensed professional.</summary>
    MedicalTreatment = 3,

    /// <summary>Lost time injury (LTI).</summary>
    LostTime = 4,

    /// <summary>Fatality.</summary>
    Fatality = 5,

    /// <summary>Property or equipment damage.</summary>
    PropertyDamage = 6,

    /// <summary>Environmental spill or chemical release.</summary>
    Environmental = 7
}
