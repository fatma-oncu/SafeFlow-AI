using System;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SafeFlow.API.Swagger;

/// <summary>
/// Swagger document filter that groups, sorts, and adds descriptive metadata to API tags,
/// providing a polished, professional layout in the Swagger UI.
/// </summary>
public sealed class TagDescriptionsDocumentFilter : IDocumentFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(swaggerDoc);
        ArgumentNullException.ThrowIfNull(context);

        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new() { Name = "Auth", Description = "Secure identity endpoints — authentication, registration, password management, and refresh token rotation." },
            new() { Name = "Users", Description = "User profile query operations, including current session profile checks and IDOR-safe details retrieval." },
            new() { Name = "Roles", Description = "Authorization capability management — role definition details and permission map settings." },
            new() { Name = "Employees", Description = "Staff member management — onboarding, status configuration, and department transfers." },
            new() { Name = "RiskAssessments", Description = "Occupational health and safety (OHS) risk assessments — hazards, risk matrix evaluations, and revisions." },
            new() { Name = "Incidents", Description = "Workplace incident logging, dynamic investigation workflows, action plans, and audit closure." }
        };
    }
}
