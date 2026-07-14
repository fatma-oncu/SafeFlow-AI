namespace SafeFlow.SharedKernel.Exceptions;

/// <summary>
/// Thrown when an operation requires authentication and no valid identity has been
/// established for the caller.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="UnauthorizedException"/> represents an authentication failure: the
/// caller's identity is absent or cannot be verified (e.g., missing or expired JWT,
/// revoked refresh token).
/// </para>
/// <para>
/// The API exception-handling middleware maps <see cref="UnauthorizedException"/> to
/// HTTP 401 Unauthorized.
/// </para>
/// <para>
/// Contrast with <see cref="ForbiddenException"/>, which signals that the caller is
/// authenticated but lacks the required permission (HTTP 403).
/// </para>
/// </remarks>
public sealed class UnauthorizedException : SafeFlowException
{
    /// <summary>
    /// Initializes a new <see cref="UnauthorizedException"/> with a default message.
    /// </summary>
    public UnauthorizedException()
        : base("Authentication is required to access this resource.")
    {
    }

    /// <summary>
    /// Initializes a new <see cref="UnauthorizedException"/> with the specified error message.
    /// </summary>
    /// <param name="message">
    /// A human-readable description of the authentication failure.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    public UnauthorizedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="UnauthorizedException"/> with the specified error message
    /// and a reference to the inner exception that caused this exception.
    /// </summary>
    /// <param name="message">
    /// A human-readable description of the authentication failure.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c>.
    /// </param>
    public UnauthorizedException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
