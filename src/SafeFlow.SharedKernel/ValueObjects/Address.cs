using SafeFlow.SharedKernel.Exceptions;

namespace SafeFlow.SharedKernel.ValueObjects;

/// <summary>
/// Represents an immutable postal address, composed of a street, city, optional
/// state or province, optional postal code, and country.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Address"/> enforces the following rules on construction:
/// <list type="bullet">
///   <item><c>Street</c> must not be <c>null</c>, empty, or whitespace and must
///   not exceed <see cref="MaxStreetLength"/> characters.</item>
///   <item><c>City</c> must not be <c>null</c>, empty, or whitespace and must
///   not exceed <see cref="MaxCityLength"/> characters.</item>
///   <item><c>Country</c> must not be <c>null</c>, empty, or whitespace and must
///   not exceed <see cref="MaxCountryLength"/> characters.</item>
///   <item><c>StateProvince</c>, when provided, must not exceed
///   <see cref="MaxStateProvinceLength"/> characters.</item>
///   <item><c>PostalCode</c>, when provided, must not exceed
///   <see cref="MaxPostalCodeLength"/> characters.</item>
/// </list>
/// </para>
/// <para>
/// Leading and trailing whitespace is trimmed from all non-null fields.
/// No geolocation, address-standardisation, or external-service validation is performed.
/// </para>
/// <para>
/// Equality is structural across all five components.
/// </para>
/// </remarks>
public sealed class Address : ValueObject
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>Maximum character length for <c>Street</c>.</summary>
    public const int MaxStreetLength = 200;

    /// <summary>Maximum character length for <c>City</c>.</summary>
    public const int MaxCityLength = 100;

    /// <summary>Maximum character length for <c>StateProvince</c>.</summary>
    public const int MaxStateProvinceLength = 100;

    /// <summary>Maximum character length for <c>PostalCode</c>.</summary>
    public const int MaxPostalCodeLength = 20;

    /// <summary>Maximum character length for <c>Country</c>.</summary>
    public const int MaxCountryLength = 100;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    private Address(
        string street,
        string city,
        string? stateProvince,
        string? postalCode,
        string country)
    {
        Street = street;
        City = city;
        StateProvince = stateProvince;
        PostalCode = postalCode;
        Country = country;
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>Gets the street line of the address (house number and street name).</summary>
    public string Street { get; }

    /// <summary>Gets the city or locality name.</summary>
    public string City { get; }

    /// <summary>
    /// Gets the state, province, or region name, or <c>null</c> when not applicable
    /// (e.g., for countries that do not use administrative subdivisions).
    /// </summary>
    public string? StateProvince { get; }

    /// <summary>
    /// Gets the postal or ZIP code, or <c>null</c> when not applicable.
    /// </summary>
    public string? PostalCode { get; }

    /// <summary>Gets the country name.</summary>
    public string Country { get; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="Address"/> value object.
    /// </summary>
    /// <param name="street">
    /// The street address line.  Must not be <c>null</c>, empty, or whitespace.
    /// Must not exceed <see cref="MaxStreetLength"/> characters.
    /// </param>
    /// <param name="city">
    /// The city or locality.  Must not be <c>null</c>, empty, or whitespace.
    /// Must not exceed <see cref="MaxCityLength"/> characters.
    /// </param>
    /// <param name="stateProvince">
    /// The state, province, or region.  Optional (<c>null</c> is accepted).
    /// Must not exceed <see cref="MaxStateProvinceLength"/> characters when provided.
    /// </param>
    /// <param name="postalCode">
    /// The postal or ZIP code.  Optional (<c>null</c> is accepted).
    /// Must not exceed <see cref="MaxPostalCodeLength"/> characters when provided.
    /// </param>
    /// <param name="country">
    /// The country name.  Must not be <c>null</c>, empty, or whitespace.
    /// Must not exceed <see cref="MaxCountryLength"/> characters.
    /// </param>
    /// <returns>A validated, immutable <see cref="Address"/> instance.</returns>
    /// <exception cref="ValidationException">
    /// Thrown when any required field is <c>null</c>, empty, or whitespace, or when
    /// any field exceeds its maximum permitted length.
    /// </exception>
    public static Address Create(
        string street,
        string city,
        string? stateProvince,
        string? postalCode,
        string country)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        var trimStreet = ValidateRequired(street, nameof(street), MaxStreetLength, errors);
        var trimCity = ValidateRequired(city, nameof(city), MaxCityLength, errors);
        var trimCountry = ValidateRequired(country, nameof(country), MaxCountryLength, errors);
        var trimState = ValidateOptional(stateProvince, nameof(stateProvince), MaxStateProvinceLength, errors);
        var trimPostal = ValidateOptional(postalCode, nameof(postalCode), MaxPostalCodeLength, errors);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new Address(trimStreet!, trimCity!, trimState, trimPostal, trimCountry!);
    }

    // -------------------------------------------------------------------------
    // Overrides
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return StateProvince;
        yield return PostalCode;
        yield return Country;
    }

    /// <summary>
    /// Returns the address formatted as a single-line string:
    /// <c>Street, City[, StateProvince] [PostalCode], Country</c>.
    /// </summary>
    public override string ToString()
    {
        var parts = new List<string> { Street, City };

        if (!string.IsNullOrWhiteSpace(StateProvince))
            parts.Add(StateProvince);

        if (!string.IsNullOrWhiteSpace(PostalCode))
            parts.Add(PostalCode);

        parts.Add(Country);

        return string.Join(", ", parts);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates a required string field, adding errors to the dictionary if invalid.
    /// Returns the trimmed value on success, or <c>null</c> when invalid.
    /// </summary>
    private static string? ValidateRequired(
        string? value,
        string fieldName,
        int maxLength,
        Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[fieldName] = [$"{fieldName} must not be empty."];
            return null;
        }

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
        {
            errors[fieldName] = [$"{fieldName} must not exceed {maxLength} characters."];
            return null;
        }

        return trimmed;
    }

    /// <summary>
    /// Validates an optional string field.  <c>null</c> input is accepted and returned
    /// as <c>null</c>.  Non-<c>null</c> values are trimmed and length-checked.
    /// </summary>
    private static string? ValidateOptional(
        string? value,
        string fieldName,
        int maxLength,
        Dictionary<string, string[]> errors)
    {
        if (value is null) return null;

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
        {
            errors[fieldName] = [$"{fieldName} must not exceed {maxLength} characters."];
            return null;
        }

        return trimmed.Length == 0 ? null : trimmed;
    }
}
