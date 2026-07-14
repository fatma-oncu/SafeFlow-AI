namespace SafeFlow.SharedKernel.Exceptions;

/// <summary>
/// Thrown when one or more input values fail domain or application-level validation rules.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ValidationException"/> carries a structured error dictionary whose keys are
/// field or property names and whose values are the corresponding error messages.
/// This format is compatible with the RFC 7807 Problem Details <c>errors</c> extension
/// field and with the ASP.NET Core model-state dictionary, allowing the API middleware
/// to map it directly to an HTTP 422 Unprocessable Entity response without reflection
/// or additional transformation.
/// </para>
/// <para>
/// This exception is intentionally not coupled to FluentValidation or any validation
/// library.  Application-layer pipeline behaviours are responsible for catching library-
/// specific validation failures and re-raising them as <see cref="ValidationException"/>.
/// </para>
/// <para>
/// The <see cref="Errors"/> dictionary is immutable after construction to preserve
/// exception integrity across async call stacks.
/// </para>
/// </remarks>
public sealed class ValidationException : SafeFlowException
{
    // -------------------------------------------------------------------------
    // Constructors
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="ValidationException"/> with a structured error
    /// dictionary.
    /// </summary>
    /// <param name="errors">
    /// A dictionary mapping field names to their associated validation error messages.
    /// Must not be <c>null</c>.  An empty dictionary is permitted when the failure is
    /// not field-specific.
    /// </param>
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation failures occurred.")
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new <see cref="ValidationException"/> for a single field failure.
    /// </summary>
    /// <param name="fieldName">
    /// The name of the field or property that failed validation.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="errorMessage">
    /// The validation error message for the field.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    public ValidationException(string fieldName, string errorMessage)
        : base("One or more validation failures occurred.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage, nameof(errorMessage));

        Errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { fieldName, [errorMessage] }
        };
    }

    /// <summary>
    /// Initializes a new <see cref="ValidationException"/> with a custom message and
    /// a structured error dictionary.
    /// </summary>
    /// <param name="message">
    /// A human-readable summary of the validation failure.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <param name="errors">
    /// A dictionary mapping field names to their associated validation error messages.
    /// Must not be <c>null</c>.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <c>null</c>.
    /// </param>
    public ValidationException(
        string message,
        IReadOnlyDictionary<string, string[]> errors,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));
        Errors = errors;
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the structured map of field names to their validation error messages.
    /// </summary>
    /// <remarks>
    /// Keys are field or property names (case-insensitive).  Values are non-empty arrays
    /// of error message strings — a field may fail multiple rules simultaneously.
    /// </remarks>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
