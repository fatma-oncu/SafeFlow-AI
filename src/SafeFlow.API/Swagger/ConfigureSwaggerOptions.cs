using System;
using System.IO;
using System.Linq;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<ConfigureSwaggerOptions> _logger;

    /// <summary>
    /// Initialises a new <see cref="ConfigureSwaggerOptions"/>.
    /// </summary>
    /// <param name="provider">The API version description provider.</param>
    /// <param name="logger">Logger for diagnostics during XML documentation loading.</param>
    public ConfigureSwaggerOptions(
        IApiVersionDescriptionProvider provider,
        ILogger<ConfigureSwaggerOptions> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _logger   = logger   ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void Configure(SwaggerGenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // Define a separate Swagger document per API version
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
                         + "Enter the token only (without the 'Bearer ' prefix)."
        });

        // ── Filters & Transformers ────────────────────────────────────────────
        // Apply security requirement dynamically based on [Authorize] attributes
        options.OperationFilter<AuthorizeCheckOperationFilter>();

        // Map RFC 7807 ProblemDetails to standard error response definitions
        options.OperationFilter<ProblemDetailsOperationFilter>();

        // Inject high-quality documentation schemas and payload examples
        options.SchemaFilter<SwaggerSchemaFilter>();

        // Configure categories, descriptions, and ordering in the Swagger UI
        options.DocumentFilter<TagDescriptionsDocumentFilter>();

        // Hide internal API endpoints marked with [InternalApi] attribute
        options.DocInclusionPredicate((_, apiDesc) =>
        {
            var hasInternalAttribute = apiDesc.ActionDescriptor.EndpointMetadata
                .Any(m => m.GetType().FullName == "SafeFlow.API.Attributes.InternalApiAttribute");

            return !hasInternalAttribute;
        });

        // ── XML Documentation ──────────────────────────────────────────────────
        // Include XML documentation files from all compiled assemblies in the base path
        var xmlFiles = Directory.GetFiles(
            AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);

        foreach (var xmlFile in xmlFiles)
        {
            try
            {
                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Swagger XML documentation file could not be loaded and will be skipped. File: {XmlFile}",
                    xmlFile);
            }
        }
    }

    private static OpenApiInfo CreateInfoForVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title       = "SafeFlow AI API Docs",
            Version     = description.ApiVersion.ToString(),
            Description = "Enterprise Identity, Safety Monitoring, and Risk Assessment Platform — API Specifications.",
            Contact = new OpenApiContact
            {
                Name  = "SafeFlow Engineering",
                Email = "api@safeflow.io",
            },
            License = new OpenApiLicense
            {
                Name = "Proprietary"
            }
        };

        if (description.IsDeprecated)
        {
            info.Description += " **[DEPRECATED] This API version has been deprecated and should not be used in new designs.**";
        }

        return info;
    }
}
