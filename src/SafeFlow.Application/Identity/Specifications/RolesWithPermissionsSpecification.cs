using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.Identity.Specifications;

/// <summary>
/// Specification to load all roles eager-loading their assigned permissions.
/// </summary>
public sealed class RolesWithPermissionsSpecification : BaseSpecification<Role>
{
    public RolesWithPermissionsSpecification()
    {
        AddInclude(r => r.RolePermissions);
        ApplyNoTracking();
    }
}
