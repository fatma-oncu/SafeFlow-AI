using Asp.Versioning.ApiExplorer;
using SafeFlow.API;
using SafeFlow.API.Extensions;
using SafeFlow.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Service Registration ──────────────────────────────────────────────────────
builder.Services.AddApi(builder.Configuration);

// ── Application Build ─────────────────────────────────────────────────────────
var app = builder.Build();

// ── Database Initialization (Development only) ────────────────────────────────
// Migrations are auto-applied in Development. In Staging/Production use:
//   dotnet ef database update --project src/SafeFlow.Infrastructure --startup-project src/SafeFlow.API
if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
}

// ── Middleware Pipeline ───────────────────────────────────────────────────────

// 1. Global exception handler — must be first
app.UseMiddleware<GlobalExceptionMiddleware>();

// 2. Security headers
app.UseHsts();
app.UseHttpsRedirection();

// 3. Swagger (development + staging)
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

// 4. Routing
app.UseRouting();

// 5. Authentication + Authorization
app.UseAuthentication();
app.UseAuthorization();

// 6. Controllers
app.MapControllers();

app.Run();

/// <summary>
/// Exposes <see cref="Program"/> as a <c>public partial class</c> so that
/// integration tests can reference the entry point via WebApplicationFactory.
/// </summary>
public partial class Program { }
