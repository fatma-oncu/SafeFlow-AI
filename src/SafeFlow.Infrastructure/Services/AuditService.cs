using Microsoft.Extensions.Logging;
using SafeFlow.Application.Identity.Interfaces;

namespace SafeFlow.Infrastructure.Services;

/// <summary>
/// ILogger-backed implementation of <see cref="IAuditService"/>.
/// </summary>
/// <remarks>
/// <para>
/// Phase 1 does not persist audit records to a dedicated store.
/// All audit events are written to the structured logging pipeline (e.g., Serilog/Seq)
/// where they are indexed and searchable.
/// </para>
/// <para>
/// A future phase will persist audit entries to the database using an
/// <c>AuditLog</c> aggregate and an outbox pattern.
/// </para>
/// <para>
/// Per the <c>SECURITY_GUIDELINES.md</c> contract, this implementation never throws.
/// Logging failures are swallowed silently so that audit logging never disrupts
/// the primary business operation.
/// </para>
/// </remarks>
internal sealed class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;

    /// <summary>Initialises a new <see cref="AuditService"/>.</summary>
    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task LogAsync(
        AuditAction action,
        bool isSuccess,
        string ipAddress,
        Guid? userId = null,
        string? email = null,
        string? failureReason = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (isSuccess)
            {
                _logger.LogInformation(
                    "[AUDIT] Action={Action} Success=true UserId={UserId} Email={Email} IP={IP} UA={UA}",
                    action, userId, email, ipAddress, userAgent);
            }
            else
            {
                _logger.LogWarning(
                    "[AUDIT] Action={Action} Success=false UserId={UserId} Email={Email} IP={IP} Reason={Reason} UA={UA}",
                    action, userId, email, ipAddress, failureReason, userAgent);
            }
        }
        catch (Exception ex)
        {
            // Swallow — audit logging must never disrupt the primary operation
            _logger.LogError(ex, "[AUDIT] Failed to write audit log entry for action {Action}", action);
        }

        return Task.CompletedTask;
    }
}
