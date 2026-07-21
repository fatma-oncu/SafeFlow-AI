using SafeFlow.SharedKernel.Entities;
using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.SharedKernel.ValueObjects;
using SafeFlow.Domain.Identity.Entities;
using SafeFlow.Domain.Identity.Events;

namespace SafeFlow.Domain.Identity.Aggregates;

/// <summary>
/// Represents an authenticated user within the SafeFlow system.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="User"/> is the primary aggregate root in the Identity bounded context.
/// All state transitions (locking, unlocking, role assignment, login recording) must
/// occur through its public domain methods to preserve invariants and emit domain events.
/// </para>
/// <para>
/// Password credential management is intentionally absent from this aggregate.
/// Credential storage and verification are delegated to ASP.NET Core Microsoft Identity,
/// which manages the <c>IdentityUser</c> table in parallel.  The <see cref="User"/>
/// aggregate holds domain-specific state only.
/// </para>
/// <para>
/// Role assignment is idempotent: assigning a role that the user already holds has no
/// effect and raises no event.
/// </para>
/// </remarks>
public sealed class User : AggregateRoot
{
    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    private readonly List<UserRole> _userRoles = [];

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Private parameterless constructor reserved for ORM materialisation.
    /// </summary>
    private User() { }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the user's validated, normalised email address.
    /// Acts as the primary login identifier.
    /// </summary>
    public Email Email { get; private set; } = default!;

    /// <summary>
    /// Gets the user's full name (first name + last name).
    /// </summary>
    public FullName FullName { get; private set; } = default!;

    /// <summary>
    /// Gets the user's phone number, or <c>null</c> when not provided.
    /// </summary>
    public PhoneNumber? PhoneNumber { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this user account is active.
    /// Inactive accounts cannot authenticate even if not explicitly locked.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this user account is locked.
    /// Locked accounts cannot authenticate until an administrator unlocks them.
    /// </summary>
    public bool IsLocked { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp of the user's most recent successful authentication,
    /// or <c>null</c> if the user has never logged in.
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// Gets the read-only set of role assignments for this user.
    /// </summary>
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new active <see cref="User"/> aggregate and raises a
    /// <see cref="UserRegisteredDomainEvent"/>.
    /// </summary>
    /// <param name="id">The unique identifier for this user.</param>
    /// <param name="email">
    /// The validated email address of the user. Must not be <c>null</c>.
    /// </param>
    /// <param name="fullName">
    /// The validated full name of the user. Must not be <c>null</c>.
    /// </param>
    /// <param name="phoneNumber">
    /// The validated phone number of the user. Optional (<c>null</c> is accepted).
    /// </param>
    /// <returns>A new, active <see cref="User"/> aggregate root instance.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="email"/> or <paramref name="fullName"/> is <c>null</c>.
    /// </exception>
    public static User Create(
        Guid id,
        Email email,
        FullName fullName,
        PhoneNumber? phoneNumber = null)
    {
        ArgumentNullException.ThrowIfNull(email, nameof(email));
        ArgumentNullException.ThrowIfNull(fullName, nameof(fullName));

        var user = new User();
        user.Id = id;
        user.Email = email;
        user.FullName = fullName;
        user.PhoneNumber = phoneNumber;
        user.IsActive = true;
        user.IsLocked = false;

        user.RaiseDomainEvent(
            new UserRegisteredDomainEvent(id, email.Value, DateTime.UtcNow));

        return user;
    }

    // -------------------------------------------------------------------------
    // Domain methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Locks this user account, preventing authentication.
    /// Idempotent: locking an already-locked account has no effect.
    /// Raises a <see cref="UserLockedDomainEvent"/>.
    /// </summary>
    /// <param name="reason">
    /// A human-readable explanation for the lockout.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="reason"/> is <c>null</c> or whitespace.
    /// </exception>
    public void Lock(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason, nameof(reason));

        if (IsLocked) return;

        IsLocked = true;
        RaiseDomainEvent(new UserLockedDomainEvent(Id, reason, DateTime.UtcNow));
    }

    /// <summary>
    /// Unlocks this user account, restoring the ability to authenticate.
    /// Idempotent: unlocking an already-unlocked account has no effect.
    /// Raises a <see cref="UserUnlockedDomainEvent"/>.
    /// </summary>
    public void Unlock()
    {
        if (!IsLocked) return;

        IsLocked = false;
        RaiseDomainEvent(new UserUnlockedDomainEvent(Id, DateTime.UtcNow));
    }

    /// <summary>
    /// Updates the <see cref="LastLoginAt"/> timestamp to the current UTC time.
    /// Raises a <see cref="UserLoggedInDomainEvent"/>.
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        RaiseDomainEvent(
            new UserLoggedInDomainEvent(Id, LastLoginAt.Value, DateTime.UtcNow));
    }

    /// <summary>
    /// Assigns the specified role to this user.
    /// Idempotent: assigning a role the user already holds raises no event and
    /// makes no change.
    /// Raises a <see cref="UserRoleAssignedDomainEvent"/> when a new assignment is made.
    /// </summary>
    /// <param name="roleId">
    /// The identifier of the role to assign. Must not be an empty <see cref="Guid"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="roleId"/> is an empty GUID.
    /// </exception>
    public void AssignRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("RoleId must not be an empty GUID.", nameof(roleId));

        bool alreadyAssigned = _userRoles.Exists(ur => ur.RoleId == roleId);
        if (alreadyAssigned) return;

        _userRoles.Add(UserRole.Create(Id, roleId));
        RaiseDomainEvent(new UserRoleAssignedDomainEvent(Id, roleId, DateTime.UtcNow));
    }

    /// <summary>
    /// Removes the specified role from this user.
    /// Idempotent: removing a role the user does not hold has no effect.
    /// Raises a <see cref="UserRoleRemovedDomainEvent"/> when a role is removed.
    /// </summary>
    /// <param name="roleId">
    /// The identifier of the role to remove. Must not be an empty <see cref="Guid"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="roleId"/> is an empty GUID.
    /// </exception>
    public void RemoveRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("RoleId must not be an empty GUID.", nameof(roleId));

        var userRole = _userRoles.Find(ur => ur.RoleId == roleId);
        if (userRole is null) return;

        _userRoles.Remove(userRole);
        RaiseDomainEvent(new UserRoleRemovedDomainEvent(Id, roleId, DateTime.UtcNow));
    }

    /// <summary>
    /// Updates the user's phone number. Pass <c>null</c> to remove the phone number.
    /// </summary>
    /// <param name="phoneNumber">The new phone number, or <c>null</c> to clear it.</param>
    public void UpdatePhoneNumber(PhoneNumber? phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

    /// <summary>
    /// Deactivates this user account. Deactivated accounts cannot authenticate.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
    }

    /// <summary>
    /// Reactivates a previously deactivated user account.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
    }
}
