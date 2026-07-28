using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SafeFlow.SharedKernel.Interfaces;

namespace SafeFlow.Infrastructure.Services;

/// <summary>
/// Resolves the currently authenticated user from the ASP.NET Core
/// <see cref="IHttpContextAccessor"/>.
/// </summary>
/// <remarks>
/// <para>
/// Never exposes <see cref="HttpContext"/> outside this class. The Application and
/// Domain layers consume the clean <see cref="ICurrentUserService"/> abstraction only.
/// </para>
/// <para>
/// Registered as <em>Scoped</em> so that every HTTP request gets a fresh snapshot
/// of the principal resolved from the JWT middleware.
/// </para>
/// </remarks>
internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initialises a new <see cref="CurrentUserService"/>.
    /// </summary>
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    /// <inheritdoc/>
    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User?.FindFirstValue("sub");

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    /// <inheritdoc/>
    public string? UserName =>
        User?.FindFirstValue(ClaimTypes.Name)
        ?? User?.FindFirstValue("name");

    /// <inheritdoc/>
    public string? Email =>
        User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue("email");

    /// <inheritdoc/>
    public IReadOnlyCollection<string> Roles =>
        User?.FindAll(ClaimTypes.Role)
             .Select(c => c.Value)
             .ToList()
             .AsReadOnly()
        ?? (IReadOnlyCollection<string>)Array.Empty<string>();

    /// <inheritdoc/>
    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;
}
