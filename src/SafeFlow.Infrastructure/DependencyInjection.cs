using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SafeFlow.Application.Employees.Interfaces;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Application.Incidents.Interfaces;
using SafeFlow.Application.RiskAssessments.Interfaces;
using SafeFlow.Infrastructure.Identity;
using SafeFlow.Infrastructure.Options;
using SafeFlow.Infrastructure.Persistence;
using SafeFlow.Infrastructure.Persistence.Repositories;
using SafeFlow.Infrastructure.Persistence.Seed;
using SafeFlow.Infrastructure.Services;
using SafeFlow.SharedKernel.Interfaces;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SafeFlow.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services with the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all Infrastructure layer services to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">Application configuration (appsettings, env vars).</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions(configuration)
            .AddDatabase(configuration)
            .AddIdentity()
            .AddJwtAuthentication(configuration)
            .AddRepositories()
            .AddApplicationServices()
            .AddHttpContextServices()
            .AddSeedServices();

        return services;
    }

    // ── Options ────────────────────────────────────────────────────────────────

    private static IServiceCollection AddOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        services.Configure<SeedSettings>(
            configuration.GetSection(SeedSettings.SectionName));

        return services;
    }

    // ── Seed services ──────────────────────────────────────────────────────────

    private static IServiceCollection AddSeedServices(
        this IServiceCollection services)
    {
        // Initializer is Singleton so it can be resolved from the root container
        // without requiring an active scope at startup.
        services.AddSingleton<DatabaseInitializer>();

        // Seeders are Scoped — they depend on DbContext and Identity services.
        services.AddScoped<RoleSeeder>();
        services.AddScoped<PermissionSeeder>();
        services.AddScoped<AdminSeeder>();

        return services;
    }

    // ── Database / EF Core ─────────────────────────────────────────────────────

    private static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SafeFlowDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(SafeFlowDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                })
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        // Domain event dispatcher — wraps IDomainEvent → INotification for MediatR
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        return services;
    }


    // ── ASP.NET Core Identity ──────────────────────────────────────────────────

    private static IServiceCollection AddIdentity(
        this IServiceCollection services)
    {
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                // Password policy (mirrors SECURITY_GUIDELINES §5)
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Lockout policy — managed at application layer, Identity is advisory
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // Email must be unique
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<SafeFlowDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    // ── JWT Bearer Authentication ──────────────────────────────────────────────

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{JwtSettings.SectionName}' is missing.");

       // ── Fail fast if the RSA key is absent ────────────────────────────────
if (string.IsNullOrWhiteSpace(jwtSettings.RsaPrivateKeyPem))
{
    throw new InvalidOperationException(
        "JWT RSA private key is not configured. " +
        "Set 'JwtSettings:RsaPrivateKeyPem' via dotnet user-secrets, " +
        "environment variable, or Key Vault before starting the application.");
}

// Extract the public-key parameters for token validation.
string privateKeyPem = jwtSettings.RsaPrivateKeyPem
    .Replace("\\r\\n", "\n")
    .Replace("\\n", "\n")
    .Trim();

RSAParameters rsaPublicParams;

using (var rsa = RSA.Create())
{
    rsa.ImportFromPem(privateKeyPem.AsSpan());
    rsaPublicParams = rsa.ExportParameters(false);
}

// Create a new RSA instance containing only the public key for validationKey
var validationRsa = RSA.Create();
validationRsa.ImportParameters(rsaPublicParams);

var validationKey = new RsaSecurityKey(validationRsa);

services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = validationKey,

            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(
                    """{"error":"Unauthorized","message":"Token is missing or invalid."}""");
            },
            OnForbidden = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync(
                    """{"error":"Forbidden","message":"You do not have permission to perform this action."}""");
            },
        };
    });

        return services;
    }

    // ── Repositories ──────────────────────────────────────────────────────────

    private static IServiceCollection AddRepositories(
        this IServiceCollection services)
    {
        // Generic repository — registered open-generic so DI resolves any TEntity
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped(typeof(IReadRepository<>), typeof(EfRepository<>));

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    // ── Application Services ───────────────────────────────────────────────────

    private static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        // Identity bridge
        services.AddScoped<IIdentityService, IdentityService>();

        // JWT token operations
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Audit (ILogger-backed in Phase 1)
        services.AddScoped<IAuditService, AuditService>();

        // Clock
        services.AddSingleton<IDateTimeService, DateTimeService>();

        // Cache (IMemoryCache-backed in Phase 1)
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // Email (dev-only placeholder in Phase 1)
        services.AddScoped<IEmailService, DevEmailService>();

        // File storage (local disk in Phase 1)
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Employee Number Generator
        services.AddScoped<IEmployeeNumberGenerator, EmployeeNumberGenerator>();

        // Risk Assessment Number Generator
        services.AddScoped<IRiskAssessmentNumberGenerator, RiskAssessmentNumberGenerator>();

        // Incident Number Generator
        services.AddScoped<IIncidentNumberGenerator, IncidentNumberGenerator>();

        return services;
    }

    // ── HTTP Context Services ──────────────────────────────────────────────────

    private static IServiceCollection AddHttpContextServices(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();

        return services;
    }
}
