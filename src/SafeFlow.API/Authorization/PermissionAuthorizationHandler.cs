using Microsoft.AspNetCore.Authorization;

namespace SafeFlow.API.Authorization;

/// <summary>
/// Custom <see cref="IAuthorizationRequirement"/> that demands the authenticated
/// principal carries a specific permission claim (<c>"permission"</c>).
/// </summary>
/// <param name="Permission">The permission value that must be present.</param>
public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;

/// <summary>
/// Evaluates <see cref="PermissionRequirement"/> against the current
/// <see cref="AuthorizationHandlerContext"/>.
/// </summary>
/// <remarks>
/// Succeeds when the principal holds a claim of type <c>"permission"</c>
/// whose value equals <see cref="PermissionRequirement.Permission"/>.
/// </remarks>
public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private const string PermissionClaimType = "permission";

    /// <inheritdoc/>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        bool hasPermission = context.User.Claims
            .Any(c => c.Type == PermissionClaimType
                   && (c.Value == requirement.Permission ||
                       c.Value.Replace('.', ':') == requirement.Permission ||
                       c.Value.Replace(':', '.') == requirement.Permission));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
