using SafeFlow.Domain.Identity.Aggregates;
using SafeFlow.SharedKernel.Specifications;

namespace SafeFlow.Application.Identity.Specifications;

/// <summary>
/// Retrieves a <see cref="User"/> aggregate by its normalised email address,
/// including the user's role assignments.
/// </summary>
public sealed class UserByEmailSpecification : BaseSpecification<User>
{
    /// <summary>
    /// Initializes the specification with the given <paramref name="email"/>.
    /// </summary>
    /// <param name="email">
    /// The email address to filter by. Case-insensitive normalisation is
    /// handled by the <c>Email</c> value object stored on the aggregate.
    /// </param>
    public UserByEmailSpecification(string email)
        : base(u => u.Email.Value == email.ToLowerInvariant().Trim())
    {
        AddInclude(u => u.UserRoles);
        ApplyNoTracking();
    }
}

/// <summary>
/// Retrieves a <see cref="User"/> aggregate by its primary key,
/// including the user's role assignments.
/// </summary>
public sealed class UserByIdWithRolesSpecification : BaseSpecification<User>
{
    /// <summary>
    /// Initializes the specification with the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    public UserByIdWithRolesSpecification(Guid userId)
        : base(u => u.Id == userId)
    {
        AddInclude(u => u.UserRoles);
        ApplyNoTracking();
    }
}

/// <summary>
/// Returns all active, non-deleted <see cref="User"/> aggregates ordered by email,
/// with paging applied.
/// </summary>
public sealed class UsersPagedSpecification : BaseSpecification<User>
{
    /// <summary>
    /// Initializes the paged specification.
    /// </summary>
    /// <param name="pageNumber">1-based page index.</param>
    /// <param name="pageSize">Maximum results per page.</param>
    public UsersPagedSpecification(int pageNumber, int pageSize)
        : base(u => !u.IsDeleted)
    {
        ApplyOrderBy(u => u.Email.Value);
        ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        ApplyNoTracking();
    }
}
