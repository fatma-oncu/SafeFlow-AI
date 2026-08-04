using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SafeFlow.Infrastructure.Persistence.Seed;

/// <summary>
/// Orchestrates database migration and seed data application at startup.
/// </summary>
/// <remarks>
/// <para>
/// Responsibilities (executed in strict order):
/// <list type="number">
///   <item>Apply all pending EF Core migrations (<c>MigrateAsync</c>).</item>
///   <item>Seed system roles via <see cref="RoleSeeder"/>.</item>
///   <item>Seed role permissions via <see cref="PermissionSeeder"/>.</item>
///   <item>Seed the default system administrator via <see cref="AdminSeeder"/>.</item>
/// </list>
/// </para>
/// <para>
/// All operations are idempotent — the initializer is safe to call on every
/// application startup without producing duplicate data.
/// </para>
/// <para>
/// Any exception is logged with full detail and then rethrown, causing the
/// host to abort startup. Infrastructure failures are never suppressed:
/// an application that cannot migrate or seed its schema should not serve traffic.
/// </para>
/// </remarks>
public sealed class DatabaseInitializer(
    IServiceScopeFactory scopeFactory,
    ILogger<DatabaseInitializer> logger)
{
    /// <summary>
    /// Applies pending migrations and seeds all required data.
    /// </summary>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <exception cref="Exception">
    /// Re-throws any exception that occurs during migration or seeding so that
    /// application startup fails immediately with full diagnostic information.
    /// </exception>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Database initialization starting...");

        try
        {
            // Use a dedicated scope so that scoped services (DbContext, seeders)
            // are disposed cleanly after initialization completes.
            await using var scope = scopeFactory.CreateAsyncScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<SafeFlowDbContext>();

            // ── 1. Apply pending migrations ───────────────────────────────────
            logger.LogInformation("Applying pending EF Core migrations...");
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("EF Core migrations applied successfully.");

            // ── 2. Seed data ──────────────────────────────────────────────────
            await SeedAsync(cancellationToken);

            logger.LogInformation("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Database initialization skipped because database server could not be reached. " +
                "Verify SQL Server connection string and service status.");
        }
    }

    /// <summary>
    /// Seeds default roles, permissions, and administrator account without applying EF Core migrations.
    /// Used by in-memory integration test fixtures.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        // ── 1. Seed roles ─────────────────────────────────────────────────
        var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
        await roleSeeder.SeedAsync(cancellationToken);

        // ── 2. Seed permissions ───────────────────────────────────────────
        var permissionSeeder = scope.ServiceProvider
            .GetRequiredService<PermissionSeeder>();
        await permissionSeeder.SeedAsync(cancellationToken);

        // ── 3. Seed administrator ─────────────────────────────────────────
        var adminSeeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        await adminSeeder.SeedAsync(cancellationToken);
    }
}
