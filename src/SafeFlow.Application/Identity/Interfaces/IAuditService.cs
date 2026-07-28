namespace SafeFlow.Application.Identity.Interfaces;

/// <summary>
/// Defines the contract for writing structured audit log entries for
/// authentication and authorisation lifecycle events.
/// </summary>
/// <remarks>
/// <para>
/// Audit entries are immutable once written. Implementations must not allow
/// update or delete operations on log records.
/// </para>
/// <para>
/// Per <c>SECURITY_GUIDELINES.md</c> § 6, all critical authentication events —
/// registration, email verification, login (success and failure), token refreshes,
/// password changes, and account lockouts — must be captured here.
/// </para>
/// <para>
/// Implementations must not throw when logging fails. Audit failures should be
/// swallowed and reported only through the structured logging infrastructure
/// (e.g., Serilog), so that a logging error never disrupts the primary operation.
/// </para>
/// </remarks>
public interface IAuditService
{
    /// <summary>
    /// Writes an audit log entry for the given authentication or authorisation event.
    /// </summary>
    /// <param name="action">The type of action being audited.</param>
    /// <param name="isSuccess">Whether the action completed successfully.</param>
    /// <param name="ipAddress">The IP address of the originating request.</param>
    /// <param name="userId">The ID of the affected user, when known.</param>
    /// <param name="email">The email of the affected user, when known.</param>
    /// <param name="failureReason">
    /// A short, non-sensitive description of why the operation failed.
    /// Must not include passwords, tokens, or PII beyond the email.
    /// </param>
    /// <param name="userAgent">The HTTP User-Agent header value, if available.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task LogAsync(
        AuditAction action,
        bool isSuccess,
        string ipAddress,
        Guid? userId = null,
        string? email = null,
        string? failureReason = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Enumerates the authentication and authorisation lifecycle events that are
/// captured in the audit log.
/// </summary>
public enum AuditAction
{
    /// <summary>A new user completed the registration flow.</summary>
    Register,

    /// <summary>An email verification message was dispatched to the user.</summary>
    EmailVerificationSent,

    /// <summary>The user successfully confirmed their email address.</summary>
    EmailVerified,

    /// <summary>A successful login was recorded.</summary>
    Login,

    /// <summary>A login attempt failed (invalid credentials or locked account).</summary>
    LoginFailed,

    /// <summary>The user explicitly logged out and the refresh token was revoked.</summary>
    Logout,

    /// <summary>A refresh token was successfully rotated and a new access token issued.</summary>
    TokenRefreshed,

    /// <summary>
    /// A revoked refresh token was presented — possible token theft detected.
    /// All tokens in the affected family were immediately revoked.
    /// </summary>
    StolenTokenDetected,

    /// <summary>The user successfully changed their password.</summary>
    PasswordChanged,

    /// <summary>A password reset link was requested (email dispatched).</summary>
    PasswordResetRequested,

    /// <summary>The user's password was successfully reset via a reset token.</summary>
    PasswordReset,

    /// <summary>An account was locked after repeated failed login attempts.</summary>
    UserLocked,

    /// <summary>A previously locked account was manually unlocked by an administrator.</summary>
    UserUnlocked,

    /// <summary>A role was assigned to a user by an administrator.</summary>
    RoleAssigned,

    /// <summary>A role was removed from a user by an administrator.</summary>
    RoleRemoved,
}
