using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.Infrastructure.Services;

/// <summary>
/// System-clock implementation of <see cref="IDateTimeService"/> backed by
/// <see cref="DateTime.UtcNow"/> and <see cref="DateTime.Now"/>.
/// </summary>
/// <remarks>
/// Registered as a <em>Singleton</em> — stateless, thread-safe, no per-request overhead.
/// Unit tests may substitute a fixed-clock implementation to make time deterministic.
/// </remarks>
internal sealed class DateTimeService : IDateTimeService
{
    /// <inheritdoc/>
    public DateTime UtcNow => DateTime.UtcNow;

    /// <inheritdoc/>
    public DateTime LocalNow => DateTime.Now;
}
