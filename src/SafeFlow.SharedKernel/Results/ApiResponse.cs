namespace SafeFlow.SharedKernel.Results;

/// <summary>
/// A transport envelope returned by the API layer wrapping any operation outcome.
/// </summary>
/// <typeparam name="T">
/// The type of the payload data. Use <c>ApiResponse&lt;object&gt;</c> or
/// <c>ApiResponse&lt;Unit&gt;</c> for operations that produce no meaningful data.
/// </typeparam>
/// <remarks>
/// <para>
/// <see cref="ApiResponse{T}"/> has no dependency on ASP.NET Core, HTTP, or any
/// infrastructure concern. It is a plain data transfer object produced in the
/// Application layer and serialized by the API layer.
/// </para>
/// <para>
/// This type is intentionally immutable: all properties are <c>init</c>-only and
/// set via the <see cref="Ok"/>, <see cref="Fail"/>, and <see cref="ValidationFail"/>
/// factory methods, which enforce a consistent construction pattern.
/// </para>
/// </remarks>
public sealed class ApiResponse<T>
{
    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    private ApiResponse(
        bool success,
        string? message,
        T? data,
        IReadOnlyList<string> errors)
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = errors;
        TimestampUtc = DateTime.UtcNow;
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// When <c>true</c>, <see cref="Data"/> is populated.
    /// When <c>false</c>, <see cref="Errors"/> contains failure details.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets an optional human-readable summary message, such as a confirmation
    /// string (e.g., <c>"User registered successfully."</c>).
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Gets the operation's result payload. <c>null</c> when <see cref="Success"/>
    /// is <c>false</c> or when the operation produces no data.
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// Gets the list of human-readable error messages. Empty on a successful response.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Gets the UTC instant at which this response was produced.
    /// Useful for debugging and distributed tracing.
    /// </summary>
    public DateTime TimestampUtc { get; }

    // -------------------------------------------------------------------------
    // Factory methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a successful response carrying a data payload.
    /// </summary>
    /// <param name="data">The result data. Must not be <c>null</c>.</param>
    /// <param name="message">An optional confirmation message.</param>
    /// <returns>A successful <see cref="ApiResponse{T}"/>.</returns>
    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));
        return new ApiResponse<T>(true, message, data, Array.Empty<string>());
    }

    /// <summary>
    /// Creates a failed response from a structured <see cref="Error"/>.
    /// </summary>
    /// <param name="error">The domain error. Must not be <see cref="Error.None"/>.</param>
    /// <returns>A failed <see cref="ApiResponse{T}"/>.</returns>
    public static ApiResponse<T> Fail(Error error)
    {
        ArgumentNullException.ThrowIfNull(error, nameof(error));
        return new ApiResponse<T>(
            false,
            error.Message,
            default,
            [error.Message]);
    }

    /// <summary>
    /// Creates a failed response from multiple validation messages.
    /// Intended for use with FluentValidation failures.
    /// </summary>
    /// <param name="errors">
    /// The collection of human-readable validation error messages. Must not be
    /// <c>null</c> or empty.
    /// </param>
    /// <returns>A failed <see cref="ApiResponse{T}"/> with validation errors.</returns>
    public static ApiResponse<T> ValidationFail(IEnumerable<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors, nameof(errors));
        var errorList = errors.ToList().AsReadOnly();

        if (errorList.Count == 0)
            throw new ArgumentException(
                "At least one validation error message must be provided.", nameof(errors));

        return new ApiResponse<T>(false, "One or more validation errors occurred.", default, errorList);
    }
}
