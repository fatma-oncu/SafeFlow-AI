namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Provides a testable abstraction over the system clock.
/// </summary>
/// <remarks>
/// <para>
/// Directly calling <see cref="DateTime.UtcNow"/> inside domain logic or application
/// handlers makes those classes non-deterministic and hard to unit-test.  By depending
/// on <see cref="IDateTimeService"/> instead, tests can inject a fixed or controllable
/// clock without patching static members.
/// </para>
/// <para>
/// Audit fields (<c>CreatedAt</c>, <c>LastModifiedAt</c>) and domain events
/// (<c>OccurredAt</c>) must always be stamped using <see cref="UtcNow"/> to ensure
/// consistent, timezone-independent record keeping across all tenants and deployments.
/// <see cref="LocalNow"/> is reserved for display-only scenarios (e.g., generating a
/// human-readable report timestamp in the tenant's local timezone).
/// </para>
/// </remarks>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current date and time expressed in Coordinated Universal Time (UTC).
    /// </summary>
    /// <remarks>
    /// Use this property for all persistence, auditing, and domain event timestamps.
    /// </remarks>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets the current date and time expressed in the server's local timezone.
    /// </summary>
    /// <remarks>
    /// Prefer <see cref="UtcNow"/> for all storage operations.  Use <see cref="LocalNow"/>
    /// only when a human-readable, timezone-aware timestamp is required for display or
    /// reporting purposes and the target timezone has been explicitly determined.
    /// </remarks>
    DateTime LocalNow { get; }
}
