namespace SafeFlow.Domain.Incidents.Enums;

/// <summary>
/// Defines severity levels for workplace incidents.
/// </summary>
public enum IncidentSeverity
{
    /// <summary>Minor incident with negligible impact or near miss.</summary>
    Minor = 1,

    /// <summary>Moderate incident requiring first aid or minor medical attention.</summary>
    Moderate = 2,

    /// <summary>Major incident requiring professional medical treatment or property damage.</summary>
    Major = 3,

    /// <summary>Critical incident resulting in lost time, hospitalization, or environmental damage.</summary>
    Critical = 4,

    /// <summary>Fatal incident resulting in loss of life.</summary>
    Fatal = 5
}
