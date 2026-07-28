using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SafeFlow.API.Swagger;

/// <summary>
/// Configures Swagger generation options for each discovered API version.
/// </summary>
/// <remarks>
/// Implements <see cref="IConfigureOptions{SwaggerGenOptions}"/> so that it is
/// resolved by the DI container and applied automatically during swagger setup.
/// A separate Swagger document is generated per <see cref="ApiVersionDescription"/>
/// so that the UI can display versioned endpoints without mixing them.
/// </remarks>
public sealed class ConfigureSwaggerOptions
    : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    /// <summary>
    /// Initialises a new <see cref="ConfigureSwaggerOptions"/>.
    /// </summary>
    /// <param name="provider">The API version description provider.</param>
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc/>
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                CreateInfoForVersion(description));
        }

        // ── JWT Bearer security definition ────────────────────────────────────
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name         = "Authorization",
            Type         = SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT",
            In           = ParameterLocation.Header,
            Description  = "JWT Authorization header using the Bearer scheme. "
                         + "Enter the token only (without 'Bearer ' prefix).",
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        });

        // Include XML documentation from all referenced assemblies
        var xmlFiles = Directory.GetFiles(
            AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);

        foreach (var xmlFile in xmlFiles)
        {
            options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
        }
    }

    private static OpenApiInfo CreateInfoForVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title       = "SafeFlow AI API",
            Version     = description.ApiVersion.ToString(),
            Description = "Enterprise identity and AI-safety platform — Identity module.",
            Contact = new OpenApiContact
            {
                Name  = "SafeFlow Engineering",
                Email = "api@safeflow.io",
            },
            License = new OpenApiLicense
            {
                Name = "Proprietary",
            },
        };

        if (description.IsDeprecated)
        {
            info.Description += " **This API version is deprecated.**";
        }

        return info;
    }
}
