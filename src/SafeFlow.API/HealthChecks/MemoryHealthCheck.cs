using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace SafeFlow.API.HealthChecks;

/// <summary>
/// Reports the managed heap memory usage of the current process.
/// </summary>
/// <remarks>
/// <para>
/// Returns <see cref="HealthStatus.Healthy"/> when allocated memory is below the
/// configured threshold, <see cref="HealthStatus.Degraded"/> when it exceeds the
/// threshold but the process is still functional.
/// </para>
/// <para>
/// This check is tagged <c>live</c> so it participates in the liveness probe
/// (<c>GET /health/live</c>) but never returns <see cref="HealthStatus.Unhealthy"/>
/// — a process is only truly dead when it cannot answer requests at all.
/// </para>
/// <para>
/// The memory threshold is read from <c>HealthChecks:MemoryThresholdMb</c>
/// in application configuration (default 512 MB).
/// </para>
/// </remarks>
public sealed class MemoryHealthCheck : IHealthCheck
{
    private const long DefaultThresholdMb = 512;

    private readonly long _thresholdBytes;

    /// <summary>
    /// Initialises a new <see cref="MemoryHealthCheck"/>.
    /// </summary>
    /// <param name="configuration">Application configuration used to read the optional threshold.</param>
    public MemoryHealthCheck(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var thresholdMb = configuration.GetValue<long>("HealthChecks:MemoryThresholdMb", DefaultThresholdMb);
        _thresholdBytes = thresholdMb * 1024 * 1024;
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var allocatedBytes = GC.GetTotalMemory(forceFullCollection: false);
        var allocatedMb    = allocatedBytes / 1024.0 / 1024.0;
        var thresholdMb    = _thresholdBytes / 1024.0 / 1024.0;

        var data = new Dictionary<string, object>
        {
            ["allocatedMb"]  = Math.Round(allocatedMb,  2),
            ["thresholdMb"]  = Math.Round(thresholdMb,  2),
            ["gen0Collections"] = GC.CollectionCount(0),
            ["gen1Collections"] = GC.CollectionCount(1),
            ["gen2Collections"] = GC.CollectionCount(2),
        };

        if (allocatedBytes >= _thresholdBytes)
        {
            return Task.FromResult(HealthCheckResult.Degraded(
                description: $"Memory usage {allocatedMb:F1} MB exceeds threshold {thresholdMb:F0} MB.",
                data: data));
        }

        return Task.FromResult(HealthCheckResult.Healthy(
            description: $"Memory usage {allocatedMb:F1} MB is within threshold {thresholdMb:F0} MB.",
            data: data));
    }
}
