namespace SafeFlow.Application.RiskAssessments.Interfaces;

/// <summary>
/// Service abstraction for generating sequential risk assessment numbers.
/// </summary>
public interface IRiskAssessmentNumberGenerator
{
    /// <summary>
    /// Generates the next sequential risk assessment number string (e.g. <c>"RA-2026-000001"</c>).
    /// </summary>
    Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default);
}
