namespace SafeFlow.Infrastructure.Options;

/// <summary>
/// Configuration settings for JWT access-token generation, bound from
/// <c>appsettings.json → JwtSettings</c>.
/// </summary>
/// <remarks>
/// <para>
/// All private keys are loaded at startup from the configuration. No secrets are
/// hardcoded. In production, the PEM content is stored in Azure Key Vault or
/// an environment variable injected by the secrets manager.
/// </para>
/// </remarks>
public sealed class JwtSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Gets or sets the RSA private key in PEM format, used to sign access tokens (RS256).
    /// Must be set via environment variable or Key Vault in production.
    /// </summary>
    public string RsaPrivateKeyPem { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the <c>iss</c> (issuer) claim embedded in every access token.
    /// Typically the public URL of the authentication service (e.g., <c>https://auth.safeflow.io</c>).
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the <c>aud</c> (audience) claim embedded in every access token.
    /// Typically the public URL of the resource API (e.g., <c>https://api.safeflow.io</c>).
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the access token lifetime in minutes. Defaults to 15.
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets the refresh token lifetime in days. Defaults to 7.
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
