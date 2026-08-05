using Asp.Versioning.ApiExplorer;
using SafeFlow.API;
using SafeFlow.API.Extensions;
using SafeFlow.API.Middleware;
using Serilog;
using Serilog.Events;

// ── Bootstrap logger ─────────────────────────────────────────────────────────
// Captures startup failures before the host and DI container are built.
// Replaced by the fully configured logger once UseSerilog() has loaded config.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("SafeFlow API starting up");

    var builder = WebApplication.CreateBuilder(args);

    // Load User Secrets explicitly if present (takes precedence over appsettings)
    builder.Configuration.AddUserSecrets<Program>(optional: true);

    // ── Serilog ───────────────────────────────────────────────────────────────
    // Reads the "Serilog" section from appsettings.json / environment overrides.
    // Two-phase initialization: bootstrap → full configuration.
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.FromLogContext());

    // ── Service Registration ──────────────────────────────────────────────────
    builder.Services.AddApi(builder.Configuration);

    // ── Application Build ─────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Database Initialization (Development only) ────────────────────────────
    // Migrations are auto-applied in Development. In Staging/Production use:
    //   dotnet ef database update --project src/SafeFlow.Infrastructure --startup-project src/SafeFlow.API
    if (app.Environment.IsDevelopment())
    {
        await app.InitializeDatabaseAsync();
    }

    // ── Middleware Pipeline ───────────────────────────────────────────────────

    // 1. Correlation ID — must be first so all downstream logs carry the ID
    app.UseMiddleware<CorrelationIdMiddleware>();

    // 2. Global exception handler — catches everything downstream
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // 3. Serilog request logging — one clean structured event per request
    //    Placed after exception middleware so failed requests are still logged.
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000} ms";

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost",   httpContext.Request.Host.Value ?? string.Empty);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("RemoteIp",      httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            diagnosticContext.Set("UserAgent",     httpContext.Request.Headers.UserAgent.ToString());

            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId",
                    httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? "unknown");
            }
        };
    });

    // 4. Security headers
    app.UseHsts();
    app.UseHttpsRedirection();

    // 5. Swagger (development + staging)
    if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
    {
        var apiVersionDescriptionProvider =
            app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"SafeFlow API {description.GroupName.ToUpperInvariant()}");
            }

            options.RoutePrefix = "swagger";
        });
    }

    // 6. Routing
    app.UseRouting();

    // 7. Authentication + Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // 8. Controllers
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "SafeFlow API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Exposes <see cref="Program"/> as a <c>public partial class</c> so that
/// integration tests can reference the entry point via WebApplicationFactory.
/// </summary>
public partial class Program { }
