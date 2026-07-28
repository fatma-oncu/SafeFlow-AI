using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;


namespace SafeFlow.Application;

/// <summary>
/// Extension methods for registering Application layer services with the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the Application layer services (MediatR, FluentValidation) to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ── MediatR ──────────────────────────────────────────────────────────
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            // Pipeline behaviours registered here in future:
            // cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            // cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        // ── FluentValidation ─────────────────────────────────────────────────
        services.AddValidatorsFromAssembly(
            typeof(DependencyInjection).Assembly,
            includeInternalTypes: true);

        return services;
    }
}
