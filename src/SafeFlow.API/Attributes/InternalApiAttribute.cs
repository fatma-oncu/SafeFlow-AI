using System;

namespace SafeFlow.API.Attributes;

/// <summary>
/// Marks an API controller or endpoint as internal, hiding it from public Swagger/OpenAPI documentation.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class InternalApiAttribute : Attribute
{
}
