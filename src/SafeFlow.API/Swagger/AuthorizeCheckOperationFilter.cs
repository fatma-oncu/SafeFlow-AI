using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SafeFlow.API.Swagger;

/// <summary>
/// Swagger operation filter that applies JWT Bearer authorization requirements and responses 
/// dynamically to endpoints based on the presence of <see cref="AuthorizeAttribute"/> and 
/// absence of <see cref="AllowAnonymousAttribute"/>.
/// </summary>
public sealed class AuthorizeCheckOperationFilter : IOperationFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        var hasAuthorize = endpointMetadata.Any(m => m is AuthorizeAttribute);
        var hasAllowAnonymous = endpointMetadata.Any(m => m is AllowAnonymousAttribute);

        if (hasAuthorize && !hasAllowAnonymous)
        {
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized - Authentication token is missing or invalid." });
            operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden - Insufficient permissions to access this endpoint." });

            operation.Security ??= new List<OpenApiSecurityRequirement>();

            var securityScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            };

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [securityScheme] = Array.Empty<string>()
            });
        }
    }
}
