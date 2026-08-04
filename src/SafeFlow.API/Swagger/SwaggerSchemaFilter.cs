using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SafeFlow.Application.Identity.DTOs;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SafeFlow.API.Swagger;

/// <summary>
/// Swagger schema filter that injects mock/production-grade schema examples 
/// for core DTOs and ProblemDetails objects.
/// </summary>
public sealed class SwaggerSchemaFilter : ISchemaFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        if (context.Type == typeof(ProblemDetails))
        {
            schema.Example = new OpenApiObject
            {
                ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                ["title"] = new OpenApiString("One or more validation errors occurred."),
                ["status"] = new OpenApiInteger(400),
                ["detail"] = new OpenApiString("Please refer to the errors property for additional details."),
                ["instance"] = new OpenApiString("/api/v1/users/me"),
                ["errors"] = new OpenApiObject
                {
                    ["Email"] = new OpenApiArray { new OpenApiString("Geçersiz e-posta adresi.") },
                    ["Password"] = new OpenApiArray { new OpenApiString("Şifre en az 8 karakter olmalıdır.") }
                }
            };
        }
        else if (context.Type == typeof(LoginResponseDto))
        {
            schema.Example = new OpenApiObject
            {
                ["accessToken"] = new OpenApiString("eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOiJhNWI1YzVkNS1lNWY1LTRhNWItYmM1ZC01ZTVmNWE1YjVjNWQiLCJlbWFpbCI6InVzZXJAc2FmZWZsb3cuaW8iLCJyb2xlcyI6WyJFbXBsb3llZSJdfQ..."),
                ["expiresIn"] = new OpenApiInteger(3600),
                ["refreshToken"] = new OpenApiString("dGhpc19pc19hX3NlY3VyZV9yZWZyZXNoX3Rva2VuX3ZhbHVlXzY0X2J5dGVzX2Jhc2U2NA=="),
                ["user"] = new OpenApiObject
                {
                    ["id"] = new OpenApiString("a5b5c5d5-e5f5-4a5b-bc5d-5e5f5a5b5c5d"),
                    ["email"] = new OpenApiString("user@safeflow.io"),
                    ["fullName"] = new OpenApiString("Ahmet Yılmaz"),
                    ["roles"] = new OpenApiArray { new OpenApiString("Employee") }
                }
            };
        }
        else if (context.Type == typeof(UserDto))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("a5b5c5d5-e5f5-4a5b-bc5d-5e5f5a5b5c5d"),
                ["email"] = new OpenApiString("user@safeflow.io"),
                ["fullName"] = new OpenApiString("Ahmet Yılmaz"),
                ["phoneNumber"] = new OpenApiString("+905551234567"),
                ["isActive"] = new OpenApiBoolean(true),
                ["isLocked"] = new OpenApiBoolean(false),
                ["roles"] = new OpenApiArray { new OpenApiString("Employee") }
            };
        }
    }
}
