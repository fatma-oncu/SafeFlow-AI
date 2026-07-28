using Microsoft.AspNetCore.Identity;

namespace SafeFlow.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity user record stored in the <c>AspNetUsers</c> table.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ApplicationUser"/> is a pure Identity concern and is completely
/// independent of the domain <c>User</c> aggregate root.  The two models are
/// bridged exclusively through <see cref="Services.IdentityService"/>, which
/// translates between Identity operations (password hashing, lockout counters,
/// etc.) and domain aggregate mutations.
/// </para>
/// <para>
/// Fields that duplicate information on the domain <c>User</c> aggregate
/// (e.g., <see cref="FirstName"/>, <see cref="LastName"/>) are stored here as
/// well so that Identity can fulfil its contractual obligations (display name
/// for email templates, lockout notifications) without having to join across
/// to the domain table on every request.
/// </para>
/// </remarks>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Gets or sets the user's given (first) name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the user's family (last) name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the tenant (Company) this user belongs to.
    /// Used to scope cross-cutting concerns without a domain dependency.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the most recent successful authentication,
    /// mirroring the domain <c>User.LastLoginAt</c> for Identity-level queries.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}
