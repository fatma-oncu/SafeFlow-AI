using SafeFlow.Infrastructure.Persistence.Seed;

namespace SafeFlow.API.Extensions;

/// <summary>
/// Extension methods for <see cref="WebApplication"/> startup configuration.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations and seeds required data.
    /// Should only be called in the <c>Development</c> environment.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In Staging and Production, migrations are applied via the CI/CD pipeline
    /// using <c>dotnet ef database update</c>, not at application startup.
    /// </para>
    /// <para>
    /// This method resolves <see cref="DatabaseInitializer"/> from the root
    /// container and runs <see cref="DatabaseInitializer.InitializeAsync"/> as
    /// a fire-and-complete operation before the HTTP pipeline starts.
    /// </para>
    /// </remarks>
    /// <param name="app">The <see cref="WebApplication"/> instance.</param>
    /// <returns>The same <see cref="WebApplication"/> for chaining.</returns>
    public static async Task<WebApplication> InitializeDatabaseAsync(
        this WebApplication app)
    {
        var initializer = app.Services.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync(app.Lifetime.ApplicationStopping);
        return app;
    }
}
