namespace SafeFlow.Application.Incidents.Interfaces;

/// <summary>
/// Service abstraction for generating sequential, formatted incident numbers (e.g. INC-2026-000001).
/// </summary>
public interface IIncidentNumberGenerator
{
    /// <summary>
    /// Generates the next available sequential incident number for the current year.
    /// </summary>
    Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default);
}
