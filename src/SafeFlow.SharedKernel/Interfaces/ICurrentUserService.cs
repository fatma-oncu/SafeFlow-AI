namespace SafeFlow.SharedKernel.Interfaces;

/// <summary>
/// Provides read-only access to the identity of the authenticated user who is
/// executing the current request.
/// </summary>
/// <remarks>
/// <para>
/// Implementations are registered with a scoped lifetime so that every HTTP request
/// (or background job) receives its own context snapshot.  The Infrastructure layer
/// resolves the values from the ASP.NET Core <c>IHttpContextAccessor</c> or the
/// background-job principal — this interface keeps the Application and Domain layers
/// completely decoupled from those concerns.
/// </para>
/// <para>
/// All string properties return <c>null</c> (not throw) when no authenticated user is
/// present.  Callers must check <see cref="IsAuthenticated"/> before reading identity
/// data to avoid accidental use of unauthenticated context.
/// </para>
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user, or <c>null</c>
    /// if the caller is not authenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the username (login name) of the currently authenticated user, or <c>null</c>
    /// if the caller is not authenticated.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets the email address of the currently authenticated user, or <c>null</c>
    /// if the caller is not authenticated.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the set of role names assigned to the currently authenticated user.
    /// Returns an empty collection when the caller is not authenticated or has no roles.
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>
    /// Gets a value indicating whether the current request is executing on behalf of
    /// an authenticated user.
    /// </summary>
    bool IsAuthenticated { get; }
}
