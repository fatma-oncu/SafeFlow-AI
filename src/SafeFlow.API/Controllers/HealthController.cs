using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SafeFlow.API.Controllers;

/// <summary>
/// Exposes liveness, readiness, and aggregate health-check endpoints for load-balancer
/// probes, Kubernetes health gates, and operational dashboards.
/// </summary>
/// <remarks>
/// <para>
/// All three endpoints are anonymous and unversioned. They return structured JSON
/// conforming to the RFC 7807-inspired health report format used across SafeFlow services.
/// </para>
/// <para>
/// Tag semantics:
/// <list type="bullet">
///   <item><c>live</c> — process is alive (liveness probe). Includes <c>self</c> and <c>memory</c> checks.</item>
///   <item><c>ready</c> — infrastructure is ready to serve traffic (readiness probe). Includes <c>self</c>, <c>memory</c>, and <c>sql-server</c>.</item>
///   <item><c>infrastructure</c> — external dependency checks (SQL Server). Included in aggregate and readiness probes.</item>
/// </list>
/// </para>
/// </remarks>
[AllowAnonymous]
[ApiVersionNeutral]
[Tags("Infrastructure")]
public sealed class HealthController : ApiControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    /// <summary>Initialises a new <see cref="HealthController"/>.</summary>
    /// <param name="healthCheckService">The ASP.NET Core health check service.</param>
    /// <param name="logger">Logger for unhealthy / degraded probe results.</param>
    public HealthController(
        HealthCheckService healthCheckService,
        ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        _logger             = logger             ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Liveness probe — confirms the process is alive and the GC is not under critical pressure.
    /// </summary>
    /// <remarks>
    /// Evaluates only the <c>live</c>-tagged checks (<c>self</c>, <c>memory</c>).
    /// Does <strong>not</strong> validate SQL Server or any external dependency.
    /// Returns <c>200 OK</c> for Healthy or Degraded; <c>503</c> for Unhealthy.
    /// </remarks>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">Process is alive.</response>
    /// <response code="503">Process is unresponsive or critically degraded.</response>
    [HttpGet("/health/live")]
    [ProducesResponseType(typeof(HealthReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthReportResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Live(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(
            r => r.Tags.Contains("live"),
            cancellationToken);

        LogIfUnhealthy(report, "liveness");

        return report.Status == HealthStatus.Unhealthy
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, BuildReport(report))
            : Ok(BuildReport(report));
    }

    /// <summary>
    /// Readiness probe — confirms the API is ready to serve traffic including SQL Server.
    /// </summary>
    /// <remarks>
    /// Evaluates the <c>ready</c>-tagged checks (<c>self</c>, <c>memory</c>, <c>sql-server</c>).
    /// Returns <c>503</c> for any Degraded or Unhealthy result.
    /// </remarks>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">All readiness checks passed.</response>
    /// <response code="503">One or more readiness checks failed or are degraded.</response>
    [HttpGet("/health/ready")]
    [ProducesResponseType(typeof(HealthReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthReportResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(
            r => r.Tags.Contains("ready"),
            cancellationToken);

        LogIfUnhealthy(report, "readiness");

        return report.Status == HealthStatus.Healthy
            ? Ok(BuildReport(report))
            : StatusCode(StatusCodes.Status503ServiceUnavailable, BuildReport(report));
    }

    /// <summary>
    /// Aggregate health endpoint — includes all registered health checks.
    /// </summary>
    /// <remarks>
    /// Evaluates every registered check regardless of tag. Use this endpoint for
    /// operational dashboards and manual inspection. Not recommended for automated probes.
    /// </remarks>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">All checks are Healthy.</response>
    /// <response code="503">One or more checks are Degraded or Unhealthy.</response>
    [HttpGet("/health")]
    [ProducesResponseType(typeof(HealthReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthReportResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Health(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);

        LogIfUnhealthy(report, "aggregate");

        return report.Status == HealthStatus.Healthy
            ? Ok(BuildReport(report))
            : StatusCode(StatusCodes.Status503ServiceUnavailable, BuildReport(report));
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Logs degraded or unhealthy probe results. Healthy probes produce no log output
    /// to avoid polluting Seq with routine traffic.
    /// </summary>
    private void LogIfUnhealthy(HealthReport report, string probeType)
    {
        if (report.Status == HealthStatus.Healthy)
        {
            return;
        }

        foreach (var (name, entry) in report.Entries)
        {
            if (entry.Status == HealthStatus.Healthy)
            {
                continue;
            }

            if (entry.Exception is not null)
            {
                _logger.LogError(
                    entry.Exception,
                    "Health check {CheckName} is {Status} during {ProbeType} probe. Description: {Description}",
                    name, entry.Status, probeType, entry.Description ?? "none");
            }
            else
            {
                _logger.LogWarning(
                    "Health check {CheckName} is {Status} during {ProbeType} probe. Description: {Description}",
                    name, entry.Status, probeType, entry.Description ?? "none");
            }
        }
    }

    private static HealthReportResponse BuildReport(HealthReport report) =>
        new(
            Status:        report.Status.ToString(),
            TotalDuration: report.TotalDuration,
            Entries:       report.Entries.ToDictionary(
                               e => e.Key,
                               e => new HealthEntryResponse(
                                   Status:      e.Value.Status.ToString(),
                                   Duration:    e.Value.Duration,
                                   Description: e.Value.Description,
                                   Data:        e.Value.Data.Count > 0
                                                    ? e.Value.Data.ToDictionary(k => k.Key, k => k.Value)
                                                    : null)));

    // ── Response shape ─────────────────────────────────────────────────────────

    /// <summary>Top-level health report returned by all three endpoints.</summary>
    /// <param name="Status">Overall health status: Healthy | Degraded | Unhealthy.</param>
    /// <param name="TotalDuration">Wall-clock time taken to evaluate all checks.</param>
    /// <param name="Entries">Individual check results keyed by check name.</param>
    public sealed record HealthReportResponse(
        string Status,
        TimeSpan TotalDuration,
        IReadOnlyDictionary<string, HealthEntryResponse> Entries);

    /// <summary>Individual health check result within the report.</summary>
    /// <param name="Status">Healthy | Degraded | Unhealthy.</param>
    /// <param name="Duration">Time taken by this individual check.</param>
    /// <param name="Description">Human-readable description from the check implementation.</param>
    /// <param name="Data">Optional structured diagnostic data provided by the check.</param>
    public sealed record HealthEntryResponse(
        string Status,
        TimeSpan Duration,
        string? Description,
        IReadOnlyDictionary<string, object>? Data);
}
