namespace SafeFlow.SharedKernel.Exceptions;

/// <summary>
/// Thrown when a requested entity or resource cannot be found.
/// </summary>
/// <remarks>
/// <para>
/// Use this exception in Application-layer query and command handlers when a
/// repository or service lookup returns no result for a given identifier.
/// </para>
/// <para>
/// The API exception-handling middleware maps <see cref="NotFoundException"/> to
/// HTTP 404 Not Found.
/// </para>
/// <para>
/// The convenience constructor overload accepting <c>entityName</c> and
/// <c>entityId</c> produces a consistent, structured message:
/// <c>{EntityName} with id '{EntityId}' was not found.</c>
/// This avoids ad-hoc message formatting scattered across handlers.
/// </para>
/// </remarks>
public sealed class NotFoundException : SafeFlowException
{
    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="NotFoundException"/> with a custom error message.
    /// </summary>
    /// <param name="message">
    /// A human-readable description of what was not found.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="NotFoundException"/> with a custom error message
    /// and a reference to the inner exception that caused this exception.
    /// </summary>
    /// <param name="message">
    /// A human-readable description of what was not found.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c>.
    /// </param>
    public NotFoundException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="NotFoundException"/> for a specific entity look-up
    /// failure, producing a standardised message.
    /// </summary>
    /// <param name="entityName">
    /// The name of the entity type that was not found (e.g., <c>"User"</c>, <c>"Company"</c>).
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="entityId">
    /// The identifier value used in the failed lookup.  Must not be <c>null</c>.
    /// </param>
    public NotFoundException(string entityName, object entityId)
        : base($"{entityName} with id '{entityId}' was not found.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName, nameof(entityName));
        ArgumentNullException.ThrowIfNull(entityId, nameof(entityId));

        EntityName = entityName;
        EntityId = entityId;
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the name of the entity type that could not be found, or <c>null</c>
    /// when the exception was constructed from a custom message.
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// Gets the identifier value used in the failed lookup, or <c>null</c> when
    /// the exception was constructed from a custom message.
    /// </summary>
    public object? EntityId { get; }
}
