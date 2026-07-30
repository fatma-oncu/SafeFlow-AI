namespace SafeFlow.Infrastructure.Options;

/// <summary>
/// Configuration settings for the database seed layer, bound from
/// <c>appsettings.json → SeedSettings</c>.
/// </summary>
/// <remarks>
/// Credentials (<see cref="AdminEmail"/> and <see cref="AdminPassword"/>) must
/// never be committed to source control.  Provide them exclusively via
/// <c>dotnet user-secrets</c> in Development and environment variables / Key Vault
/// in Production.
/// </remarks>
public sealed class SeedSettings
{
    /// <summary>The configuration section name.</summary>
    public const string SectionName = "SeedSettings";

    /// <summary>
    /// Gets or sets the email address for the default system administrator account.
    /// Must be supplied via <c>dotnet user-secrets</c>.
    /// </summary>
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the initial password for the default system administrator account.
    /// Must be supplied via <c>dotnet user-secrets</c>.
    /// </summary>
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name of the default administrator.
    /// Defaults to <c>"System"</c> if not overridden.
    /// </summary>
    public string AdminFirstName { get; set; } = "System";

    /// <summary>
    /// Gets or sets the last name of the default administrator.
    /// Defaults to <c>"Administrator"</c> if not overridden.
    /// </summary>
    public string AdminLastName { get; set; } = "Administrator";
}
