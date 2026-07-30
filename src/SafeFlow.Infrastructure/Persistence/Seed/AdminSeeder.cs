using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.Infrastructure.Identity;
using SafeFlow.Infrastructure.Options;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Infrastructure.Persistence.Seed;

/// <summary>
/// Seeds the default system administrator into both the Domain model and
/// ASP.NET Core Identity.
/// </summary>
/// <remarks>
/// <para>
/// Credentials are loaded exclusively from configuration (User Secrets,
/// environment variables, Azure Key Vault, etc.) and are never hardcoded.
/// </para>
/// <para>
/// The seeder is fully idempotent and safe to execute multiple times.
/// </para>
/// <para>
/// Domain invariants are preserved by assigning the role through
/// <see cref="User.AssignRole(Guid)"/> instead of manipulating EF Core
/// join entities directly.
/// </para>
/// </remarks>
internal sealed class AdminSeeder(
    SafeFlowDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IOptions<SeedSettings> seedOptions,
    ILogger<AdminSeeder> logger)
{
    /// <summary>
    /// Seeds the default administrator account.
    /// </summary>
    internal async Task SeedAsync(CancellationToken cancellationToken)
    {
        var settings = seedOptions.Value;

        if (string.IsNullOrWhiteSpace(settings.AdminEmail) ||
            string.IsNullOrWhiteSpace(settings.AdminPassword))
        {
            logger.LogWarning(
                "Admin seeding skipped because SeedSettings are not configured.");
            return;
        }

        // ---------------------------------------------------------------------
        // Idempotency
        // ---------------------------------------------------------------------
        if (await dbContext.DomainUsers.AnyAsync(
                x => x.Id == SystemConstants.SystemAdminUserId,
                cancellationToken))
        {
            logger.LogDebug(
                "System administrator already exists. Skipping seeding.");

            return;
        }

        // ---------------------------------------------------------------------
        // Validate prerequisites
        // ---------------------------------------------------------------------
        bool superAdminRoleExists = await dbContext.DomainRoles
            .AnyAsync(
                r => r.Id == SystemConstants.SuperAdminRoleId,
                cancellationToken);

        if (!superAdminRoleExists)
        {
            logger.LogError(
                "SuperAdmin role not found. RoleSeeder must execute before AdminSeeder.");

            return;
        }

        // ---------------------------------------------------------------------
        // Domain aggregate
        // ---------------------------------------------------------------------
        var domainUser = User.Create(
            SystemConstants.SystemAdminUserId,
            Email.Create(settings.AdminEmail),
            FullName.Create(
                settings.AdminFirstName,
                settings.AdminLastName));

        domainUser.AssignRole(SystemConstants.SuperAdminRoleId);

        await dbContext.DomainUsers.AddAsync(domainUser, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // ---------------------------------------------------------------------
        // ASP.NET Identity user
        // ---------------------------------------------------------------------
        var identityUser = new ApplicationUser
        {
            Id = SystemConstants.SystemAdminUserId,
            UserName = settings.AdminEmail,
            Email = settings.AdminEmail,
            NormalizedUserName = settings.AdminEmail.ToUpperInvariant(),
            NormalizedEmail = settings.AdminEmail.ToUpperInvariant(),
            EmailConfirmed = true,
            TenantId = SystemConstants.SystemTenantId,
            FirstName = settings.AdminFirstName,
            LastName = settings.AdminLastName,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var createResult = await userManager.CreateAsync(
            identityUser,
            settings.AdminPassword);

        if (!createResult.Succeeded)
        {
            logger.LogError(
                "Failed to create Identity administrator: {Errors}",
                string.Join("; ", createResult.Errors.Select(x => x.Description)));

            return;
        }

        var roleResult = await userManager.AddToRoleAsync(
            identityUser,
            SystemConstants.SuperAdminRoleName);

        if (!roleResult.Succeeded)
        {
            logger.LogError(
                "Failed to assign SuperAdmin Identity role: {Errors}",
                string.Join("; ", roleResult.Errors.Select(x => x.Description)));

            return;
        }

        logger.LogInformation(
            "System administrator seeded successfully ({Email}).",
            settings.AdminEmail);
    }
}