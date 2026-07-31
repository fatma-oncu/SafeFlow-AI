namespace SafeFlow.Domain.Incidents.Enums;

/// <summary>
/// Root cause categories identified upon investigation completion.
/// </summary>
public enum InvestigationResult
{
    /// <summary>Inconclusive / pending evidence.</summary>
    Inconclusive = 1,

    /// <summary>Human error or procedural violation.</summary>
    HumanError = 2,

    /// <summary>Equipment breakdown or hardware failure.</summary>
    EquipmentFailure = 3,

    /// <summary>Process flaw or inadequate safe operating procedure.</summary>
    ProcessFailure = 4,

    /// <summary>Environmental or weather factor.</summary>
    EnvironmentalFactor = 5,

    /// <summary>Third-party contractor or external factor.</summary>
    ThirdPartyFactor = 6
}
