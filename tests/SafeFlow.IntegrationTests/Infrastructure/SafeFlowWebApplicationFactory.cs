using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SafeFlow.Infrastructure.Persistence;

namespace SafeFlow.IntegrationTests.Infrastructure;

/// <summary>
/// Test-scoped <see cref="WebApplicationFactory{TEntryPoint}"/> that replaces the
/// SQL Server database with an open SQLite in-memory connection, wires a generated
/// RSA key for JWT, and configures test settings for startup.
/// </summary>
public sealed class SafeFlowWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly string TestRsaPrivateKeyPem = GenerateRsaPem();
    private readonly SqliteConnection _connection;

    public SafeFlowWebApplicationFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Inject required configuration settings into the WebHost builder before Program.cs builds services
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=(localdb)\\mssqllocaldb;Database=SafeFlowTest;");
        builder.UseSetting("JwtSettings:RsaPrivateKeyPem", TestRsaPrivateKeyPem);
        builder.UseSetting("JwtSettings:Issuer", "safeflow-test");
        builder.UseSetting("JwtSettings:Audience", "safeflow-test-audience");
        builder.UseSetting("JwtSettings:AccessTokenExpirationMinutes", "60");
        builder.UseSetting("JwtSettings:RefreshTokenExpirationDays", "7");
        builder.UseSetting("SeedSettings:AdminEmail", "testadmin@safeflow.io");
        builder.UseSetting("SeedSettings:AdminPassword", "TestAdmin@Pass1!");
        builder.UseSetting("SeedSettings:AdminFirstName", "Test");
        builder.UseSetting("SeedSettings:AdminLastName", "Admin");

        builder.ConfigureServices(services =>
        {
            // ── Remove all existing DbContext descriptors ─────────────────────
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<SafeFlowDbContext>) ||
                            d.ServiceType == typeof(DbContextOptions) ||
                            d.ServiceType.Name.Contains("DbContextOptions") ||
                            d.ServiceType == typeof(SafeFlowDbContext))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // ── Add SQLite in-memory DbContext using open connection ─────────
            services.AddDbContext<SafeFlowDbContext>(options =>
                options.UseSqlite(_connection));

            // ── Ensure schema is created ──────────────────────────────────────
            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SafeFlowDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }

    private static string GenerateRsaPem()
    {
        using var rsa = RSA.Create(2048);
        return rsa.ExportRSAPrivateKeyPem();
    }
}
