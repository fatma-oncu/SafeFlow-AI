namespace SafeFlow.SharedKernel.Results;

/// <summary>
/// Classifies the semantic category of an <see cref="Error"/>.
/// The Application and API layers use this value to translate domain failures into
/// the correct HTTP status code without coupling the Domain to HTTP semantics.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// One or more input fields failed business or format validation.
    /// Typically maps to HTTP 422 Unprocessable Entity.
    /// </summary>
    Validation,

    /// <summary>
    /// The requested resource or aggregate could not be found.
    /// Typically maps to HTTP 404 Not Found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The requested operation conflicts with the current state of the resource
    /// (e.g., duplicate email, concurrent modification).
    /// Typically maps to HTTP 409 Conflict.
    /// </summary>
    Conflict,

    /// <summary>
    /// The caller is not authenticated.
    /// Typically maps to HTTP 401 Unauthorized.
    /// </summary>
    Unauthorized,

    /// <summary>
    /// The caller is authenticated but lacks the required permission.
    /// Typically maps to HTTP 403 Forbidden.
    /// </summary>
    Forbidden,

    /// <summary>
    /// A domain business rule was violated (e.g., cannot enrol an already-certified employee).
    /// Typically maps to HTTP 400 Bad Request.
    /// </summary>
    Business,

    /// <summary>
    /// An unexpected or unclassified failure occurred.
    /// Typically maps to HTTP 500 Internal Server Error.
    /// </summary>
    Unexpected,
}
