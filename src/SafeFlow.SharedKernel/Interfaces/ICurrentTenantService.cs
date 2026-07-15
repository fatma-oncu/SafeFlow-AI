namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Provides read-only access to the identity of the active tenant (Company) for the
/// current request or job execution context.
/// </summary>
/// <remarks>
/// <para>
/// In the SafeFlow multi-tenant architecture, every tenant maps to exactly one
/// <c>Company</c> aggregate root.  The Infrastructure layer resolves the active
/// tenant from the authenticated user's JWT claims (or a request header for
/// internal service calls) and exposes it through this interface.
/// </para>
/// <para>
/// Application-layer command and query handlers use <see cref="TenantId"/> to
/// scope repository queries and to stamp new aggregates with the correct
/// tenant identifier, without holding a reference to HTTP or EF Core primitives.
/// </para>
/// <para>
/// Both properties return <c>null</c> when no tenant context is available (e.g.,
/// unauthenticated requests or system-level operations that span all tenants).
/// </para>
/// </remarks>
public interface ICurrentTenantService
{
    /// <summary>
    /// Gets the unique identifier of the active tenant (Company), or <c>null</c>
    /// if no tenant context has been established for the current request.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Gets the display name of the active tenant, or <c>null</c>
    /// if no tenant context has been established for the current request.
    /// </summary>
    string? TenantName { get; }
}
