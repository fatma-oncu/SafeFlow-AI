using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SafeFlow.Application.Identity.Interfaces;
using SafeFlow.Infrastructure.Options;

namespace SafeFlow.Infrastructure.Services;

/// <summary>
/// RS256-based JWT access-token and refresh-token service.
/// </summary>
/// <remarks>
/// <para>
/// Access tokens are signed with an RSA-SHA256 private key loaded from
/// <see cref="JwtSettings.RsaPrivateKeyPem"/>. The RSA key pair is loaded once at
/// construction time and reused for all signing operations to avoid per-request
/// key parsing overhead.
/// </para>
/// <para>
/// Refresh tokens are 64-byte cryptographically random values encoded as URL-safe
/// Base64. Only the SHA-256 hex-digest is stored in the database; the raw value
/// is returned to the caller once and never persisted.
/// </para>
/// </remarks>
internal sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    /// <summary>
    /// Initialises a new <see cref="JwtTokenService"/>.
    /// </summary>
    /// <param name="options">Bound JWT configuration.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="JwtSettings.RsaPrivateKeyPem"/> is absent or invalid.
    /// </exception>
    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(_settings.RsaPrivateKeyPem))
        {
            throw new InvalidOperationException(
                "JWT RSA private key is not configured. " +
                "Set JwtSettings:RsaPrivateKeyPem via environment variable or Key Vault.");
        }
    }

    /// <inheritdoc/>
    public int AccessTokenExpirationMinutes => _settings.AccessTokenExpirationMinutes;

    /// <inheritdoc/>
    public int RefreshTokenExpirationDays => _settings.RefreshTokenExpirationDays;

    /// <inheritdoc/>
    public string GenerateAccessToken(
        Guid userId,
        string email,
        string fullName,
        Guid tenantId,
        IEnumerable<string> roles,
        IEnumerable<string> permissions)
    {
        var now = DateTime.UtcNow;
        var expiry = now.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Name, fullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new("tenant_id", tenantId.ToString()),
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        using var tempRsa = RSA.Create();
        tempRsa.ImportFromPem(_settings.RsaPrivateKeyPem.AsSpan());
        var rsaParams = tempRsa.ExportParameters(includePrivateParameters: true);
        var signingKey = new RsaSecurityKey(rsaParams);
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiry,
            NotBefore = now,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = signingCredentials,
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }

    /// <inheritdoc/>
    public string GenerateRefreshToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <inheritdoc/>
    public string HashToken(string token)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(token);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}