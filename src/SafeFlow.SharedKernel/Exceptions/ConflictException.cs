namespace SafeFlow.SharedKernel.Exceptions;

/// <summary>
/// Thrown when an operation conflicts with the current state of a resource.
/// </summary>
/// <remarks>
/// <para>
/// Common causes include: duplicate registration (e.g., email already in use),
/// optimistic concurrency violations, or domain rules that reject a state transition
/// because the aggregate is already in a conflicting state.
/// </para>
/// <para>
/// The API exception-handling middleware maps <see cref="ConflictException"/> to
/// HTTP 409 Conflict.
/// </para>
/// </remarks>
public sealed class ConflictException : SafeFlowException
{
    /// <summary>
    /// Initializes a new <see cref="ConflictException"/> with the specified error message.
    /// </summary>
    /// <param name="message">
    /// A human-readable description of the conflict.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    public ConflictException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConflictException"/> with the specified error message
    /// and a reference to the inner exception that caused this exception.
    /// </summary>
    /// <param name="message">
    /// A human-readable description of the conflict.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c>.
    /// </param>
    public ConflictException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
