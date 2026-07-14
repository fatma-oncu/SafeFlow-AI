namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Marks a domain entity as belonging to a specific tenant (Company) in the
/// multi-tenant SafeFlow platform.
/// </summary>
/// <remarks>
/// For MVP, one tenant maps directly to one <c>Company</c> aggregate root.
/// The <see cref="TenantId"/> value equals <c>Company.Id</c>.
/// The Infrastructure layer uses this interface to apply EF Core Global Query
/// Filters that automatically scope every query to the active tenant, preventing
/// cross-tenant data leakage.
/// </remarks>
public interface ITenantEntity
{
    /// <summary>
    /// Gets the unique identifier of the tenant (Company) that owns this entity.
    /// </summary>
    Guid TenantId { get; }
}
