using SafeFlow.SharedKernel.Entities;

namespace SafeFlow.Domain.Identity.Entities;

/// <summary>
/// Represents the many-to-many join between a <c>User</c> and a <c>Role</c>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="UserRole"/> is a child entity of the <c>User</c> aggregate root.
/// It must only be created through <c>User.AssignRole</c> — never constructed directly
/// by callers outside the <c>SafeFlow.Domain</c> assembly.
/// </para>
/// <para>
/// The <c>CreatedAt</c> and <c>CreatedBy</c> audit fields inherited from
/// <see cref="BaseEntity"/> are populated by the Infrastructure layer's
/// <c>SaveChangesAsync</c> interceptor, recording when and by whom the role
/// was assigned.
/// </para>
/// </remarks>
public sealed class UserRole : BaseEntity
{
    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Private parameterless constructor reserved for ORM materialisation.
    /// </summary>
    private UserRole() { }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the identifier of the <c>User</c> aggregate that holds this role.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets the identifier of the <c>Role</c> aggregate that is assigned to the user.
    /// </summary>
    public Guid RoleId { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="UserRole"/> assignment.
    /// Intended for use only by the <c>User</c> aggregate root.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user receiving the role. Must not be an empty <see cref="Guid"/>.
    /// </param>
    /// <param name="roleId">
    /// The identifier of the role being assigned. Must not be an empty <see cref="Guid"/>.
    /// </param>
    /// <returns>A new, unsaved <see cref="UserRole"/> entity.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="userId"/> or <paramref name="roleId"/> is an empty GUID.
    /// </exception>
    internal static UserRole Create(Guid userId, Guid roleId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId must not be an empty GUID.", nameof(userId));
        if (roleId == Guid.Empty)
            throw new ArgumentException("RoleId must not be an empty GUID.", nameof(roleId));

        var entity = new UserRole();
        entity.Id = Guid.NewGuid();
        entity.UserId = userId;
        entity.RoleId = roleId;
        return entity;
    }
}
