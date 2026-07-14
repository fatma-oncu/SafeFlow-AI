namespace SafeFlow.SharedKernel.Results;

/// <summary>
/// Represents the outcome of a domain operation that does not produce a value.
/// A result is either a success or a failure carrying a structured <see cref="Error"/>.
/// </summary>
/// <remarks>
/// <para>
/// Use this type instead of throwing exceptions for expected business failures
/// (invalid input, entity not found, business rule violations).  Reserve exceptions
/// for truly unexpected, unrecoverable situations.
/// </para>
/// <para>
/// The internal constructor prevents callers from creating instances in invalid states
/// (a successful result with an error, or a failed result without one).
/// Use the static factory methods <see cref="Success()"/> and <see cref="Failure"/> to
/// create instances.
/// </para>
/// </remarks>
public class Result
{
    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="Result"/>.
    /// </summary>
    /// <param name="isSuccess">
    /// <c>true</c> for a successful result; <c>false</c> for a failure.
    /// </param>
    /// <param name="error">
    /// The error associated with a failure. Must be <see cref="Error.None"/> when
    /// <paramref name="isSuccess"/> is <c>true</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="isSuccess"/> is <c>true</c> and
    /// <paramref name="error"/> is not <see cref="Error.None"/>, or when
    /// <paramref name="isSuccess"/> is <c>false</c> and <paramref name="error"/>
    /// is <see cref="Error.None"/>.
    /// </exception>
    protected Result(bool isSuccess, Error error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));

        if (isSuccess && error != Error.None)
            throw new ArgumentException(
                "A successful result cannot carry an error.", nameof(error));

        if (!isSuccess && error == Error.None)
            throw new ArgumentException(
                "A failed result must carry a meaningful error.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with a failed result.
    /// Returns <see cref="Error.None"/> for a successful result.
    /// </summary>
    public Error Error { get; }

    // -------------------------------------------------------------------------
    // Static factory methods — non-generic
    // -------------------------------------------------------------------------

    /// <summary>Creates a successful <see cref="Result"/>.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Creates a failed <see cref="Result"/> carrying the given <paramref name="error"/>.</summary>
    /// <param name="error">The error describing the failure. Must not be <see cref="Error.None"/>.</param>
    public static Result Failure(Error error) => new(false, error);

    // -------------------------------------------------------------------------
    // Static factory methods — generic helpers (convenience overloads)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a successful <see cref="Result{TValue}"/> carrying the given value.
    /// </summary>
    /// <typeparam name="TValue">The type of the result value.</typeparam>
    /// <param name="value">The value to wrap. Must not be <c>null</c>.</param>
    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    /// <summary>
    /// Creates a failed <see cref="Result{TValue}"/> carrying the given <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the (absent) result value.</typeparam>
    /// <param name="error">The error describing the failure. Must not be <see cref="Error.None"/>.</param>
    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
}
