using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.Infrastructure.Identity;

namespace SafeFlow.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds the built-in system roles into both the Domain and ASP.NET Core
/// Identity stores.
/// </summary>
/// <remarks>
/// Domain roles and Identity roles share identical deterministic GUIDs defined
/// in <see cref="SystemConstants"/>. The seeder is fully idempotent and may be
/// executed safely on every application startup.
/// </remarks>
internal sealed class RoleSeeder(
    SafeFlowDbContext dbContext,
    RoleManager<ApplicationRole> roleManager,
    ILogger<RoleSeeder> logger)
{
    private static readonly RoleSeed[] Roles =
    [
        new(
            SystemConstants.SuperAdminRoleId,
            SystemConstants.SuperAdminRoleName,
            "Full system access — may perform any operation.",
            true),

        new(
            SystemConstants.AdminRoleId,
            SystemConstants.AdminRoleName,
            "Administrative access — manages users and roles.",
            true),

        new(
            SystemConstants.ManagerRoleId,
            SystemConstants.ManagerRoleName,
            "Management access — read-only on users and roles.",
            true),

        new(
            SystemConstants.EmployeeRoleId,
            SystemConstants.EmployeeRoleName,
            "Standard user access — read own profile only.",
            true),
    ];

    /// <summary>
    /// Seeds all built-in roles into both persistence models.
    /// </summary>
    internal async Task SeedAsync(CancellationToken cancellationToken)
    {
        await SeedDomainRolesAsync(cancellationToken);
        await SeedIdentityRolesAsync();
    }

    private async Task SeedDomainRolesAsync(CancellationToken cancellationToken)
    {
        HashSet<Guid> existingIds =
        [
            .. await dbContext.DomainRoles
                .AsNoTracking()
                .Select(r => r.Id)
                .ToListAsync(cancellationToken)
        ];

        bool changes = false;

        foreach (var role in Roles)
        {
            if (!existingIds.Add(role.Id))
            {
                logger.LogDebug(
                    "Domain role '{RoleName}' already exists.",
                    role.Name);

                continue;
            }

            await dbContext.DomainRoles.AddAsync(
                Role.Create(
                    role.Id,
                    role.Name,
                    role.Description,
                    role.IsSystemRole),
                cancellationToken);

            changes = true;

            logger.LogInformation(
                "Seeded domain role '{RoleName}' ({RoleId}).",
                role.Name,
                role.Id);
        }

        if (changes)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedIdentityRolesAsync()
    {
        foreach (var role in Roles)
        {
            if (await roleManager.FindByIdAsync(role.Id.ToString()) is not null)
            {
                logger.LogDebug(
                    "Identity role '{RoleName}' already exists.",
                    role.Name);

                continue;
            }

            var identityRole = new ApplicationRole
            {
                Id = role.Id,
                Name = role.Name,
                NormalizedName = role.Name.ToUpperInvariant(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };

            IdentityResult result = await roleManager.CreateAsync(identityRole);

            if (result.Succeeded)
            {
                logger.LogInformation(
                    "Seeded Identity role '{RoleName}' ({RoleId}).",
                    role.Name,
                    role.Id);

                continue;
            }

            logger.LogError(
                "Failed to seed Identity role '{RoleName}'. Errors: {Errors}",
                role.Name,
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    private sealed record RoleSeed(
        Guid Id,
        string Name,
        string Description,
        bool IsSystemRole);
}