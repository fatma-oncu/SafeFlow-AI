namespace SafeFlow.SharedKernel.Results;

/// <summary>
/// Represents the outcome of a domain operation that produces a value of type
/// <typeparamref name="TValue"/> on success.
/// </summary>
/// <typeparam name="TValue">The type of the value returned on success.</typeparam>
/// <remarks>
/// <para>
/// Access <see cref="Value"/> only when <see cref="Result.IsSuccess"/> is <c>true</c>.
/// Attempting to read <see cref="Value"/> on a failure result throws
/// <see cref="InvalidOperationException"/> to surface programming errors early.
/// </para>
/// <para>
/// The implicit conversion from <typeparamref name="TValue"/> allows handlers to return
/// a plain value and have it automatically wrapped in a successful result:
/// <code>Result&lt;Guid&gt; result = newId;</code>
/// When the value is <c>null</c>, the conversion produces a failure carrying
/// <see cref="Error.NullValue"/>.
/// </para>
/// </remarks>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    // -------------------------------------------------------------------------
    // Constructor (internal — use Result.Success<T> / Result.Failure<T>)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initializes a new <see cref="Result{TValue}"/>.
    /// </summary>
    /// <param name="value">The value; may be <c>null</c> for failure results.</param>
    /// <param name="isSuccess">Whether the result represents a success.</param>
    /// <param name="error">The associated error; must be <see cref="Error.None"/> on success.</param>
    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the value returned by the successful operation.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessed on a failed result. Always check
    /// <see cref="Result.IsSuccess"/> before reading this property.
    /// </exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access the value of a failed result. Error: {Error}");

    // -------------------------------------------------------------------------
    // Implicit conversion
    // -------------------------------------------------------------------------

    /// <summary>
    /// Implicitly wraps a <typeparamref name="TValue"/> in a successful result.
    /// When <paramref name="value"/> is <c>null</c>, returns a failure carrying
    /// <see cref="Error.NullValue"/>.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null
            ? Success(value)
            : Failure<TValue>(Error.NullValue);
}
