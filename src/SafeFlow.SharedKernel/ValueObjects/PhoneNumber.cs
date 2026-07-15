using SafeFlow.SharedKernel.Exceptions;

namespace SafeFlow.SharedKernel.ValueObjects;

/// <summary>
/// Represents a validated, normalised international telephone number.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PhoneNumber"/> enforces the following rules on construction:
/// <list type="bullet">
///   <item>Value must not be <c>null</c>, empty, or whitespace.</item>
///   <item>After stripping separators, the number must contain between 7 and
///   20 digit characters (inclusive).</item>
///   <item>The normalised value must consist of an optional leading
///   <c>+</c> followed exclusively by digits.</item>
/// </list>
/// </para>
/// <para>
/// Normalisation steps applied automatically:
/// <list type="bullet">
///   <item>Leading and trailing whitespace is trimmed.</item>
///   <item>Internal spaces, hyphens, dots, and parentheses are removed.</item>
///   <item>A leading <c>+</c> sign is preserved.</item>
/// </list>
/// </para>
/// <para>
/// No third-party phone-number library (e.g., libphonenumber) is used.
/// Validation is performed exclusively with BCL character checks.
/// </para>
/// </remarks>
public sealed class PhoneNumber : ValueObject
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>The minimum number of digits required in a phone number.</summary>
    public const int MinDigits = 7;

    /// <summary>
    /// The maximum total length of the normalised phone number string (including any
    /// leading <c>+</c>), as recommended by the ITU-T E.164 standard.
    /// </summary>
    public const int MaxLength = 20;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    private PhoneNumber(string value) => Value = value;

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the normalised phone number string.  Always in the form of an optional
    /// leading <c>+</c> followed by digits only (e.g., <c>+905321234567</c>).
    /// </summary>
    public string Value { get; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="PhoneNumber"/> value object from the given raw string.
    /// </summary>
    /// <param name="phoneNumber">
    /// The raw phone number string.  Spaces, hyphens, dots, and parentheses are
    /// stripped automatically.  A leading <c>+</c> is preserved.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <returns>A normalised, validated <see cref="PhoneNumber"/> instance.</returns>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="phoneNumber"/> is <c>null</c>, whitespace, contains
    /// invalid characters after normalisation, has fewer than <see cref="MinDigits"/>
    /// digit characters, or exceeds <see cref="MaxLength"/> characters.
    /// </exception>
    public static PhoneNumber Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ValidationException(nameof(PhoneNumber), "Phone number must not be empty.");

        var normalised = Normalise(phoneNumber.Trim());

        if (normalised.Length > MaxLength)
            throw new ValidationException(
                nameof(PhoneNumber),
                $"Phone number must not exceed {MaxLength} characters after normalisation.");

        if (!IsValidFormat(normalised, out int digitCount))
            throw new ValidationException(
                nameof(PhoneNumber),
                $"'{phoneNumber}' is not a valid phone number. " +
                "Only digits, an optional leading '+', spaces, hyphens, dots, " +
                "and parentheses are permitted.");

        if (digitCount < MinDigits)
            throw new ValidationException(
                nameof(PhoneNumber),
                $"Phone number must contain at least {MinDigits} digits.");

        return new PhoneNumber(normalised);
    }

    // -------------------------------------------------------------------------
    // Overrides
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>Returns the normalised phone number string.</summary>
    public override string ToString() => Value;

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Strips all separator characters (spaces, hyphens, dots, parentheses) while
    /// preserving a leading <c>+</c> sign.
    /// </summary>
    private static string Normalise(string raw)
    {
        var chars = new System.Text.StringBuilder(raw.Length);

        for (int i = 0; i < raw.Length; i++)
        {
            char c = raw[i];

            // Preserve leading + only at position 0.
            if (c == '+' && i == 0)
            {
                chars.Append(c);
                continue;
            }

            // Strip known separators.
            if (c == ' ' || c == '-' || c == '.' || c == '(' || c == ')')
                continue;

            chars.Append(c);
        }

        return chars.ToString();
    }

    /// <summary>
    /// Validates that the normalised string contains only an optional leading <c>+</c>
    /// followed by digits.  Outputs the total digit count.
    /// </summary>
    private static bool IsValidFormat(string normalised, out int digitCount)
    {
        digitCount = 0;
        int start = normalised.StartsWith('+') ? 1 : 0;

        for (int i = start; i < normalised.Length; i++)
        {
            if (!char.IsAsciiDigit(normalised[i]))
                return false;

            digitCount++;
        }

        return true;
    }
}
