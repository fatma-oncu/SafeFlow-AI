namespace SafeFlow.Application.Identity.DTOs;

/// <summary>
/// Represents a token pair returned after a successful token rotation.
/// </summary>
/// <remarks>
/// Distinct from <see cref="LoginResponseDto"/> in that it carries no user profile
/// information — the client already has the user context from the initial login.
/// </remarks>
public sealed record TokenResponseDto
{
    /// <summary>Gets the newly issued RS256-signed JWT access token.</summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>Gets the number of seconds until the new access token expires.</summary>
    public int ExpiresIn { get; init; }

    /// <summary>
    /// Gets the newly issued raw refresh token.
    /// <c>null</c> for browser clients — delivered via <c>HttpOnly</c> cookie by the API layer.
    /// </summary>
    public string? RefreshToken { get; init; }
}
