using Microsoft.AspNetCore.Identity;

namespace SafeFlow.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity role record stored in the <c>AspNetRoles</c> table.
/// </summary>
/// <remarks>
/// Kept minimal — role management at the application level is handled by the
/// domain <c>Role</c> aggregate.  This class exists solely to satisfy the
/// <see cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext{TUser,TRole,TKey}"/> generic contract.
/// </remarks>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>Initialises a new <see cref="ApplicationRole"/> with no name.</summary>
    public ApplicationRole() { }

    /// <summary>Initialises a new <see cref="ApplicationRole"/> with the given name.</summary>
    /// <param name="roleName">The role name.</param>
    public ApplicationRole(string roleName) : base(roleName) { }
}
