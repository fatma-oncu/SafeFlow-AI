using System.Net.Mail;
using SafeFlow.SharedKernel.Exceptions;

namespace SafeFlow.SharedKernel.ValueObjects;

/// <summary>
/// Represents a validated, normalised electronic mail address.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Email"/> enforces the following rules on construction:
/// <list type="bullet">
///   <item>Value must not be <c>null</c>, empty, or whitespace.</item>
///   <item>Value must not exceed 254 characters after normalisation (RFC 5321).</item>
///   <item>Value must conform to a valid RFC 5322 email address format.</item>
/// </list>
/// </para>
/// <para>
/// Normalisation steps applied automatically:
/// <list type="bullet">
///   <item>Leading and trailing whitespace is trimmed.</item>
///   <item>The entire address is converted to lowercase to ensure
///   case-insensitive equality comparisons.</item>
/// </list>
/// </para>
/// <para>
/// Format validation is delegated to the BCL's
/// <see cref="MailAddress"/> constructor, which implements RFC 5322 parsing.
/// No external libraries or regex packages are used.
/// </para>
/// </remarks>
public sealed class Email : ValueObject
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The maximum permitted length of an email address in characters,
    /// as defined by RFC 5321.
    /// </summary>
    public const int MaxLength = 254;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    private Email(string value) => Value = value;

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the normalised (lowercase, trimmed) email address string.
    /// </summary>
    public string Value { get; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="Email"/> value object from the given raw address string.
    /// </summary>
    /// <param name="email">
    /// The raw email address.  Leading and trailing whitespace is trimmed automatically.
    /// Must not be <c>null</c> or whitespace.
    /// </param>
    /// <returns>A normalised, validated <see cref="Email"/> instance.</returns>
    /// <exception cref="ValidationException">
    /// Thrown when <paramref name="email"/> is <c>null</c>, whitespace, exceeds
    /// <see cref="MaxLength"/> characters, or does not conform to a valid email format.
    /// </exception>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException(nameof(Email), "Email address must not be empty.");

        var normalised = email.Trim().ToLowerInvariant();

        if (normalised.Length > MaxLength)
            throw new ValidationException(
                nameof(Email),
                $"Email address must not exceed {MaxLength} characters.");

        if (!IsValidFormat(normalised))
            throw new ValidationException(
                nameof(Email),
                $"'{email}' is not a valid email address.");

        return new Email(normalised);
    }

    // -------------------------------------------------------------------------
    // Overrides
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>Returns the normalised email address string.</summary>
    public override string ToString() => Value;

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates the email format using the BCL <see cref="MailAddress"/> parser.
    /// Returns <c>false</c> if the address is syntactically invalid.
    /// </summary>
    private static bool IsValidFormat(string email)
    {
        try
        {
            var parsed = new MailAddress(email);
            // MailAddress may accept display-name formats like "User <user@example.com>".
            // We only accept bare address form, so the parsed address must equal the input.
            return string.Equals(parsed.Address, email, StringComparison.Ordinal);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
