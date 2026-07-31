using Microsoft.EntityFrameworkCore;
using SafeFlow.Application.RiskAssessments.Interfaces;
using SafeFlow.Infrastructure.Persistence;

namespace SafeFlow.Infrastructure.Services;

/// <summary>
/// Infrastructure service for generating sequential Risk Assessment numbers.
/// Format: <c>RA-YYYY-XXXXXX</c> (e.g., <c>"RA-2026-000001"</c>).
/// </summary>
internal sealed class RiskAssessmentNumberGenerator(SafeFlowDbContext dbContext)
    : IRiskAssessmentNumberGenerator
{
    public async Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default)
    {
        int year = DateTime.UtcNow.Year;
        string prefix = $"RA-{year}-";

        // Query highest existing sequence for the current year
        var highestNumber = await dbContext.RiskAssessments
            .IgnoreQueryFilters()
            .Where(r => r.AssessmentNumber.Value.StartsWith(prefix))
            .Select(r => r.AssessmentNumber.Value)
            .OrderByDescending(n => n)
            .FirstOrDefaultAsync(cancellationToken);

        int nextSequence = 1;

        if (!string.IsNullOrEmpty(highestNumber) && highestNumber.Length >= prefix.Length + 6)
        {
            string suffix = highestNumber[prefix.Length..];
            if (int.TryParse(suffix, out int parsed))
            {
                nextSequence = parsed + 1;
            }
        }

        return $"{prefix}{nextSequence:D6}";
    }
}
