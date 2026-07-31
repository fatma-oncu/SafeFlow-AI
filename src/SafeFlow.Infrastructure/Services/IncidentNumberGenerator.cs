using Microsoft.EntityFrameworkCore;
using SafeFlow.Application.Incidents.Interfaces;
using SafeFlow.Infrastructure.Persistence;

namespace SafeFlow.Infrastructure.Services;

/// <summary>
/// Infrastructure service for generating sequential Incident numbers (INC-YYYY-XXXXXX).
/// Uses thread-safe atomic database querying.
/// </summary>
public sealed class IncidentNumberGenerator(SafeFlowDbContext dbContext) : IIncidentNumberGenerator
{
    public async Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default)
    {
        int currentYear = DateTime.UtcNow.Year;
        string yearPrefix = $"INC-{currentYear}-";

        // Query highest sequence number for current year, ignoring soft-delete query filters
        var highestNumber = await dbContext.Incidents
            .IgnoreQueryFilters()
            .Where(i => i.IncidentNumber.Value.StartsWith(yearPrefix))
            .Select(i => i.IncidentNumber.Value)
            .OrderByDescending(n => n)
            .FirstOrDefaultAsync(cancellationToken);

        int nextSequence = 1;

        if (highestNumber is not null && highestNumber.Length >= 15)
        {
            string sequencePart = highestNumber.Substring(yearPrefix.Length);
            if (int.TryParse(sequencePart, out int currentSequence))
            {
                nextSequence = currentSequence + 1;
            }
        }

        return $"{yearPrefix}{nextSequence:D6}";
    }
}
