namespace SafeFlow.Application.Employees.Interfaces;

/// <summary>
/// Abstraction for generating unique employee numbers.
/// </summary>
public interface IEmployeeNumberGenerator
{
    /// <summary>
    /// Generates the next sequential formatted employee number asynchronously.
    /// </summary>
    Task<string> GenerateNextNumberAsync(CancellationToken cancellationToken = default);
}
