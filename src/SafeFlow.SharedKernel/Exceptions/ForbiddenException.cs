namespace SafeFlow.SharedKernel.Exceptions;

/// <summary>
/// Thrown when an authenticated caller lacks the required permission to perform
/// an operation.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ForbiddenException"/> represents an authorisation failure: the caller's
/// identity is known (they are authenticated) but they do not hold the role or
/// claim needed for the requested operation.
/// </para>
/// <para>
/// The API exception-handling middleware maps <see cref="ForbiddenException"/> to
/// HTTP 403 Forbidden.
/// </para>
/// <para>
/// Contrast with <see cref="UnauthorizedException"/>, which signals that the caller's
/// identity has not been established (HTTP 401).
/// </para>
/// </remarks>
public sealed class ForbiddenException : SafeFlowException
{
    /// <summary>
    /// Initializes a new <see cref="ForbiddenException"/> with a default message.
    /// </summary>
    public ForbiddenException()
        : base("You do not have permission to perform this action.")
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ForbiddenException"/> with the specified error message.
    /// </summary>
    /// <param name="message">
    /// A human-readable description of the access denial.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    public ForbiddenException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ForbiddenException"/> with the specified error message
    /// and a reference to the inner exception that caused this exception.
    /// </summary>
    /// <param name="message">
    /// A human-readable description of the access denial.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c>.
    /// </param>
    public ForbiddenException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
