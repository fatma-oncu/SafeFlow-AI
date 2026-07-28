using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace SafeFlow.API.Controllers;

/// <summary>
/// Liveness and readiness health-check endpoints.
/// </summary>
/// <remarks>
/// These endpoints are NOT versioned and do NOT require authentication.
/// They are intended for load-balancer and Kubernetes probes.
/// </remarks>
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class HealthController : ApiControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    /// <summary>Initialises a new <see cref="HealthController"/>.</summary>
    public HealthController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>Liveness probe — returns 200 when the process is running.</summary>
    [HttpGet("/health/live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Live() => Ok(new { status = "Alive" });

    /// <summary>
    /// Readiness probe — returns 200 when all registered health checks pass.
    /// </summary>
    [HttpGet("/health/ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(
            r => r.Tags.Contains("ready"),
            cancellationToken);

        return report.Status == HealthStatus.Healthy
            ? Ok(BuildReport(report))
            : StatusCode(StatusCodes.Status503ServiceUnavailable, BuildReport(report));
    }

    /// <summary>
    /// General health endpoint — includes all registered health checks.
    /// </summary>
    [HttpGet("/health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Health(CancellationToken cancellationToken)
    {
        var report = await _healthCheckService.CheckHealthAsync(cancellationToken);

        return report.Status == HealthStatus.Healthy
            ? Ok(BuildReport(report))
            : StatusCode(StatusCodes.Status503ServiceUnavailable, BuildReport(report));
    }

    private static object BuildReport(HealthReport report) => new
    {
        status = report.Status.ToString(),
        checks = report.Entries.Select(e => new
        {
            name        = e.Key,
            status      = e.Value.Status.ToString(),
            description = e.Value.Description,
        }),
    };
}
