using SafeFlow.SharedKernel.Results;

namespace SafeFlow.Application.Identity.Interfaces;

/// <summary>
/// Defines the contract for identity and credential management operations.
/// Implemented exclusively in the Infrastructure layer via ASP.NET Core Identity.
/// </summary>
/// <remarks>
/// <para>
/// All password hashing, credential verification, and email confirmation token
/// generation are delegated to the Infrastructure implementation so that the
/// Application layer remains free of cryptographic and framework dependencies.
/// </para>
/// <para>
/// The <see cref="IsEmailUniqueAsync"/> method is intentionally named to reflect a
/// positive check rather than an existence check, to keep domain language neutral
/// and to simplify call-site readability.
/// </para>
/// </remarks>
public interface IIdentityService
{
    /// <summary>
    /// Creates a new Identity user with the given credentials.
    /// Returns the new user's <see cref="Guid"/> on success.
    /// </summary>
    /// <param name="email">The validated email address.</param>
    /// <param name="password">The raw (unhashed) password to set.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="phoneNumber">Optional phone number.</param>
    /// <param name="tenantId">The tenant this user belongs to.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>A <see cref="Result{Guid}"/> containing the new user ID on success.</returns>
    Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string? phoneNumber,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the given credentials against the stored password hash.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="password">The raw password to verify.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>
    /// <see cref="Result{Boolean}"/> where <c>true</c> means the credentials are valid.
    /// Returns a failure result (not an exception) for invalid credentials.
    /// </returns>
    Task<Result<bool>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the password for the given user, verifying the current password first.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="currentPassword">The existing raw password for verification.</param>
    /// <param name="newPassword">The new raw password to set.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    Task<Result> ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a short-lived password reset token for the given user.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>A <see cref="Result{String}"/> containing the opaque reset token.</returns>
    Task<Result<string>> GeneratePasswordResetTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the user's password using a previously issued reset token.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="token">The reset token previously issued to the user.</param>
    /// <param name="newPassword">The new raw password to set.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    Task<Result> ResetPasswordAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the given email address is not already registered.
    /// </summary>
    /// <param name="email">The normalised email address to check.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns><c>true</c> if the email is unique (not registered); otherwise <c>false</c>.</returns>
    Task<bool> IsEmailUniqueAsync(
        string email,
        CancellationToken cancellationToken = default);
}
