namespace SafeFlow.SharedKernel.Exceptions;

/// <summary>
/// Abstract base class for all domain and application exceptions in the SafeFlow system.
/// </summary>
/// <remarks>
/// <para>
/// All SafeFlow-specific exceptions must inherit from this class rather than
/// throwing <see cref="Exception"/> or framework-specific exceptions directly.
/// This allows the API exception-handling middleware to catch every SafeFlow failure
/// with a single <c>catch (SafeFlowException)</c> clause and translate it to a
/// structured HTTP problem response without coupling the Domain or Application layers
/// to ASP.NET Core.
/// </para>
/// <para>
/// This class is <c>abstract</c> to prevent direct instantiation.  Callers must
/// throw a concrete, semantically accurate subclass
/// (<see cref="NotFoundException"/>, <see cref="ValidationException"/>, etc.).
/// This enforces meaningful failure categorisation at the call site.
/// </para>
/// <para>
/// SafeFlow exceptions should be thrown for <em>expected</em> domain and application
/// failures — not for system faults (disk full, network unreachable).  Reserve
/// plain <see cref="Exception"/> and its BCL subtypes for truly unexpected conditions.
/// </para>
/// </remarks>
public abstract class SafeFlowException : Exception
{
    /// <summary>
    /// Initializes a new <see cref="SafeFlowException"/> with the specified error message.
    /// </summary>
    /// <param name="message">
    /// A human-readable description of the error.  Must not be <c>null</c> or empty.
    /// </param>
    protected SafeFlowException(string message)
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
    }

    /// <summary>
    /// Initializes a new <see cref="SafeFlowException"/> with the specified error message
    /// and a reference to the inner exception that caused this exception.
    /// </summary>
    /// <param name="message">
    /// A human-readable description of the error.  Must not be <c>null</c> or empty.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c> if no
    /// inner exception is specified.
    /// </param>
    protected SafeFlowException(string message, Exception? innerException)
        : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
    }
}
