namespace SafeFlow.SharedKernel.Results;

/// <summary>
/// Represents a structured, immutable description of a failure in the domain.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Error"/> instances are created exclusively via the static factory methods
/// (<see cref="Validation"/>, <see cref="NotFound"/>, <see cref="Conflict"/>, etc.)
/// to ensure <see cref="Code"/> is always non-empty and <see cref="Type"/> is always
/// semantically accurate.
/// </para>
/// <para>
/// <see cref="None"/> is a sentinel value used internally by <see cref="Result"/> to
/// represent the absence of an error on a successful result.  It must never be used as
/// a meaningful failure value — use a typed factory method instead.
/// </para>
/// <para>
/// Error equality is based on <see cref="Code"/> and <see cref="Type"/> only; the
/// human-readable <see cref="Message"/> is excluded from equality to allow message
/// improvements without breaking equality contracts.
/// </para>
/// </remarks>
public sealed class Error : IEquatable<Error>
{
    // -------------------------------------------------------------------------
    // Sentinels
    // -------------------------------------------------------------------------

    /// <summary>
    /// Represents the absence of an error. Used exclusively on successful
    /// <see cref="Result"/> instances.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Unexpected);

    /// <summary>
    /// Represents a null-value error used by the implicit <c>Result&lt;T&gt;</c>
    /// conversion when a <c>null</c> value is supplied.
    /// </summary>
    public static readonly Error NullValue = new(
        "Error.NullValue",
        "A null value was provided where a non-null result was required.",
        ErrorType.Unexpected);

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the unique, machine-readable error code.
    /// Convention: <c>Domain.Entity.Reason</c> (e.g., <c>User.NotFound</c>).
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets a human-readable description of the error intended for logging
    /// and developer diagnostics. Must not be displayed directly to end-users.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the semantic category of this error, used by the API layer to select
    /// the correct HTTP status code.
    /// </summary>
    public ErrorType Type { get; }

    // -------------------------------------------------------------------------
    // Static factory methods
    // -------------------------------------------------------------------------

    /// <summary>Creates a <see cref="ErrorType.Validation"/> error.</summary>
    /// <param name="code">Machine-readable error code. Must not be null or whitespace.</param>
    /// <param name="message">Human-readable error description.</param>
    public static Error Validation(string code, string message)
        => Create(code, message, ErrorType.Validation);

    /// <summary>Creates a <see cref="ErrorType.NotFound"/> error.</summary>
    /// <param name="code">Machine-readable error code. Must not be null or whitespace.</param>
    /// <param name="message">Human-readable error description.</param>
    public static Error NotFound(string code, string message)
        => Create(code, message, ErrorType.NotFound);

    /// <summary>Creates a <see cref="ErrorType.Conflict"/> error.</summary>
    /// <param name="code">Machine-readable error code. Must not be null or whitespace.</param>
    /// <param name="message">Human-readable error description.</param>
    public static Error Conflict(string code, string message)
        => Create(code, message, ErrorType.Conflict);

    /// <summary>Creates a <see cref="ErrorType.Unauthorized"/> error.</summary>
    /// <param name="code">Machine-readable error code. Must not be null or whitespace.</param>
    /// <param name="message">Human-readable error description.</param>
    public static Error Unauthorized(string code, string message)
        => Create(code, message, ErrorType.Unauthorized);

    /// <summary>Creates a <see cref="ErrorType.Forbidden"/> error.</summary>
    /// <param name="code">Machine-readable error code. Must not be null or whitespace.</param>
    /// <param name="message">Human-readable error description.</param>
    public static Error Forbidden(string code, string message)
        => Create(code, message, ErrorType.Forbidden);

    /// <summary>Creates a <see cref="ErrorType.Business"/> error.</summary>
    /// <param name="code">Machine-readable error code. Must not be null or whitespace.</param>
    /// <param name="message">Human-readable error description.</param>
    public static Error Business(string code, string message)
        => Create(code, message, ErrorType.Business);

    /// <summary>Creates a <see cref="ErrorType.Unexpected"/> error.</summary>
    /// <param name="code">Machine-readable error code. Must not be null or whitespace.</param>
    /// <param name="message">Human-readable error description.</param>
    public static Error Unexpected(string code, string message)
        => Create(code, message, ErrorType.Unexpected);

    // -------------------------------------------------------------------------
    // Equality
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public bool Equals(Error? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Code == other.Code && Type == other.Type;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as Error);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Code, Type);

    /// <summary>Returns <c>true</c> when both errors are equal.</summary>
    public static bool operator ==(Error? left, Error? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>Returns <c>true</c> when the two errors are not equal.</summary>
    public static bool operator !=(Error? left, Error? right) => !(left == right);

    /// <inheritdoc/>
    public override string ToString() => $"[{Type}] {Code}: {Message}";

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static Error Create(string code, string message, ErrorType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));
        ArgumentException.ThrowIfNullOrWhiteSpace(message, nameof(message));
        return new Error(code, message, type);
    }
}
