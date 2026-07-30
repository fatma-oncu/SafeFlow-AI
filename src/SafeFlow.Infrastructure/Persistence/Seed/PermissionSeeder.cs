using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.Domain.Identity.Entities;

namespace SafeFlow.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds all system permissions into the domain <see cref="Role"/> aggregates.
/// </summary>
/// <remarks>
/// <para>
/// Permissions are assigned via the <see cref="Role.AddPermission"/> domain method,
/// which is idempotent — adding a permission already present has no effect.
/// The seeder therefore only needs to ensure the roles exist before calling it.
/// </para>
/// <para>
/// The permission catalogue is defined in <see cref="SystemPermissions"/>.
/// </para>
/// </remarks>
internal sealed class PermissionSeeder(
    SafeFlowDbContext dbContext,
    ILogger<PermissionSeeder> logger)
{
    /// <summary>Seeds all role-permission assignments. Safe to call multiple times.</summary>
    internal async Task SeedAsync(CancellationToken cancellationToken)
    {
        // Load roles with their current permissions in a single query
        var roles = await dbContext.DomainRoles
            .Include(r => r.RolePermissions)
            .ToListAsync(cancellationToken);

        if (roles.Count == 0)
        {
            logger.LogWarning(
                "PermissionSeeder: no roles found. Run RoleSeeder first.");
            return;
        }

        var assignments = new Dictionary<Guid, IReadOnlyList<Domain.Identity.ValueObjects.Permission>>
        {
            [SystemConstants.SuperAdminRoleId] = SystemPermissions.SuperAdminPermissions(),
            [SystemConstants.AdminRoleId]      = SystemPermissions.AdminPermissions(),
            [SystemConstants.ManagerRoleId]    = SystemPermissions.ManagerPermissions(),
            [SystemConstants.EmployeeRoleId]   = SystemPermissions.EmployeePermissions(),
        };

        bool changes = false;

        foreach (var role in roles)
        {
            if (!assignments.TryGetValue(role.Id, out var permissions)) continue;

            foreach (var permission in permissions)
            {
                bool alreadyHas = role.RolePermissions.Any(rp =>
                    rp.Permission.Module == permission.Module &&
                    rp.Permission.Action == permission.Action);

                if (!alreadyHas)
                {
                    role.AddPermission(permission);
                    changes = true;
                }
            }

            logger.LogDebug(
                "Seeding {Count} permissions for role '{Name}'.",
                permissions.Count, role.Name);
        }

        if (changes)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Permission seeding completed.");
    }
}
