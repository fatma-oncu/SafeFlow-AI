using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by EF Core tooling (<c>dotnet ef migrations add</c>,
/// <c>dotnet ef database update</c>, etc.) to construct a
/// <see cref="SafeFlowDbContext"/> instance without a running ASP.NET Core host.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="SafeFlowDbContext"/> requires <see cref="ICurrentUserService"/> and
/// <see cref="IDomainEventDispatcher"/> which are normally resolved from the DI
/// container. At design time no container exists, so this factory provides
/// lightweight no-op implementations purely for schema-inspection purposes.
/// </para>
/// <para>
/// The connection string is read from <c>appsettings.Development.json</c> or —
/// preferably — from <c>dotnet user-secrets</c> under the
/// <c>ConnectionStrings:DefaultConnection</c> key.
/// </para>
/// </remarks>
public sealed class SafeFlowDbContextFactory
    : IDesignTimeDbContextFactory<SafeFlowDbContext>
{
    /// <inheritdoc/>
    public SafeFlowDbContext CreateDbContext(string[] args)
    {
        // Connection string is read from environment variable or appsettings files.
        // For migration generation, set the env var first:
        //   $env:ConnectionStrings__DefaultConnection = "Server=...;Database=...;"
        //   dotnet ef migrations add InitialCreate --project src/SafeFlow.Infrastructure --startup-project src/SafeFlow.API
        var configuration = new ConfigurationBuilder()
            .SetBasePath(FindApiProjectDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. " +
                "Set it via environment variable before running dotnet ef: " +
                "$env:ConnectionStrings__DefaultConnection = \"<your-connection-string>\"");

        var optionsBuilder = new DbContextOptionsBuilder<SafeFlowDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsAssembly(typeof(SafeFlowDbContext).Assembly.FullName);
        });

        return new SafeFlowDbContext(
            optionsBuilder.Options,
            new DesignTimeCurrentUserService(),
            new DesignTimeDomainEventDispatcher());
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Walks up the directory tree from the current working directory to find
    /// the <c>SafeFlow.API</c> project directory (which contains
    /// <c>appsettings.json</c>).
    /// </summary>
    private static string FindApiProjectDirectory()
    {
        // EF Tools sets CWD to the Infrastructure project directory.
        // Walk up until we find the solution root, then navigate to the API project.
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current is not null)
        {
            var sln = current.GetFiles("*.sln").FirstOrDefault();
            if (sln is not null)
            {
                var apiDir = Path.Combine(current.FullName, "src", "SafeFlow.API");
                if (Directory.Exists(apiDir)) return apiDir;
            }

            current = current.Parent;
        }

        // Fallback to current directory if solution not found
        return Directory.GetCurrentDirectory();
    }

    // ── Design-time stubs ─────────────────────────────────────────────────────

    /// <summary>
    /// No-op <see cref="ICurrentUserService"/> for design-time context construction.
    /// </summary>
    private sealed class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid? UserId           => null;
        public string? UserName       => "design-time";
        public string? Email          => null;
        public IReadOnlyCollection<string> Roles => [];
        public bool IsAuthenticated   => false;
    }

    /// <summary>
    /// No-op <see cref="IDomainEventDispatcher"/> for design-time context construction.
    /// </summary>
    private sealed class DesignTimeDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(
            IReadOnlyList<SafeFlow.SharedKernel.Events.IDomainEvent> events,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
