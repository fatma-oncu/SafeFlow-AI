using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.Infrastructure.Services;

/// <summary>
/// Resolves the active tenant from the authenticated user's JWT claims via
/// <see cref="IHttpContextAccessor"/>.
/// </summary>
/// <remarks>
/// The <c>tenant_id</c> custom claim is embedded in the access token by
/// <see cref="JwtTokenService.GenerateAccessToken"/> and resolved here on every request.
/// Never exposes <see cref="HttpContext"/> outside this class.
/// </remarks>
internal sealed class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initialises a new <see cref="CurrentTenantService"/>.
    /// </summary>
    public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    /// <inheritdoc/>
    public Guid? TenantId
    {
        get
        {
            var value = User?.FindFirstValue("tenant_id");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    /// <inheritdoc/>
    public string? TenantName =>
        User?.FindFirstValue("tenant_name");
}
