using Microsoft.EntityFrameworkCore;
using SafeFlow.Application.Employees.Interfaces;
using SafeFlow.Infrastructure.Persistence;

namespace SafeFlow.Infrastructure.Services;

/// <summary>
/// Infrastructure service for generating formatted, sequential employee numbers (e.g., <c>"EMP-2026-0001"</c>).
/// </summary>
public sealed class EmployeeNumberGenerator(SafeFlowDbContext dbContext) : IEmployeeNumberGenerator
{
    public async Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default)
    {
        int year = DateTime.UtcNow.Year;

        // Query maximum sequence index for current year
        int currentCount = await dbContext.Employees
            .IgnoreQueryFilters()
            .CountAsync(cancellationToken);

        int nextSequence = currentCount + 1;
        return $"EMP-{year}-{nextSequence:D4}";
    }
}
