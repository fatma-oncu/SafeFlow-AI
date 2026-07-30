using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using SafeFlow.API.Authorization;
using SafeFlow.API.Swagger;
using SafeFlow.Application;
using SafeFlow.Infrastructure;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SafeFlow.API;

/// <summary>
/// Extension methods for registering API layer services with the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds all API-layer services: controllers, versioning, Swagger, authorization,
    /// health checks, and problem details.
    /// </summary>
    public static IServiceCollection AddApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddInfrastructure(configuration)
            .AddApiVersioningServices()
            .AddSwaggerServices()
            .AddAuthorizationPolicies()
            .AddApiControllers()
            .AddHealthCheckServices(configuration)
            .AddProblemDetailsServices();

        return services;
    }

    // ── API versioning ────────────────────────────────────────────────────────

    private static IServiceCollection AddApiVersioningServices(
        this IServiceCollection services)
    {
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat           = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }

    // ── Swagger ───────────────────────────────────────────────────────────────

    private static IServiceCollection AddSwaggerServices(
        this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen();
        services.AddEndpointsApiExplorer();

        return services;
    }

    // ── Authorization (permission-based) ─────────────────────────────────────

    private static IServiceCollection AddAuthorizationPolicies(
        this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy(Permissions.UsersRead,         p => p.AddRequirements(new PermissionRequirement(Permissions.UsersRead)))
            .AddPolicy(Permissions.UsersWrite,        p => p.AddRequirements(new PermissionRequirement(Permissions.UsersWrite)))
            .AddPolicy(Permissions.RolesRead,         p => p.AddRequirements(new PermissionRequirement(Permissions.RolesRead)))
            .AddPolicy(Permissions.RolesAssign,       p => p.AddRequirements(new PermissionRequirement(Permissions.RolesAssign)))
            .AddPolicy(Permissions.RolesRevoke,       p => p.AddRequirements(new PermissionRequirement(Permissions.RolesRevoke)))
            .AddPolicy(Permissions.EmployeesRead,     p => p.AddRequirements(new PermissionRequirement(Permissions.EmployeesRead)))
            .AddPolicy(Permissions.EmployeesCreate,   p => p.AddRequirements(new PermissionRequirement(Permissions.EmployeesCreate)))
            .AddPolicy(Permissions.EmployeesUpdate,   p => p.AddRequirements(new PermissionRequirement(Permissions.EmployeesUpdate)))
            .AddPolicy(Permissions.EmployeesDelete,   p => p.AddRequirements(new PermissionRequirement(Permissions.EmployeesDelete)))
            .AddPolicy(Permissions.EmployeesTransfer, p => p.AddRequirements(new PermissionRequirement(Permissions.EmployeesTransfer)));

        return services;
    }

    // ── Controllers ───────────────────────────────────────────────────────────

    private static IServiceCollection AddApiControllers(
        this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.PropertyNamingPolicy =
                    System.Text.Json.JsonNamingPolicy.CamelCase;
                opts.JsonSerializerOptions.DefaultIgnoreCondition =
                    System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

        return services;
    }

    // ── Health checks ─────────────────────────────────────────────────────────

    private static IServiceCollection AddHealthCheckServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running."),
                tags: ["ready", "live"]);

        return services;
    }

    // ── Problem details ───────────────────────────────────────────────────────

    private static IServiceCollection AddProblemDetailsServices(
        this IServiceCollection services)
    {
        services.AddProblemDetails();
        return services;
    }
}
