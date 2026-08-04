using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SafeFlow.API.Swagger;

/// <summary>
/// Swagger operation filter that ensures standard error responses (400, 401, 403, 404, 500) 
/// map to the RFC 7807 <see cref="ProblemDetails"/> specification schema.
/// </summary>
public sealed class ProblemDetailsOperationFilter : IOperationFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        var errorDescriptions = new Dictionary<string, string>
        {
            ["400"] = "Bad Request - The request parameters were invalid or business rules were violated.",
            ["401"] = "Unauthorized - The authentication token is missing, expired, or invalid.",
            ["403"] = "Forbidden - The authenticated user does not have the required permissions.",
            ["404"] = "Not Found - The requested resource could not be found.",
            ["500"] = "Internal Server Error - An unexpected error occurred on the server."
        };

        foreach (var (statusCode, description) in errorDescriptions)
        {
            if (operation.Responses.TryGetValue(statusCode, out var response))
            {
                response.Description = description;

                // Ensure the response maps to application/problem+json using the ProblemDetails schema
                if (!response.Content.ContainsKey("application/problem+json"))
                {
                    var problemSchema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository);
                    response.Content["application/problem+json"] = new OpenApiMediaType
                    {
                        Schema = problemSchema
                    };
                }
            }
        }
    }
}
