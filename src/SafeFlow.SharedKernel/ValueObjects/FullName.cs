using SafeFlow.SharedKernel.Exceptions;

namespace SafeFlow.SharedKernel.ValueObjects;

/// <summary>
/// Represents a person's validated full name, composed of a first name and a
/// last name.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="FullName"/> enforces the following rules on construction:
/// <list type="bullet">
///   <item><c>FirstName</c> must not be <c>null</c>, empty, or whitespace.</item>
///   <item><c>LastName</c> must not be <c>null</c>, empty, or whitespace.</item>
///   <item>Each name component must not exceed <see cref="MaxComponentLength"/>
///   characters after trimming.</item>
/// </list>
/// </para>
/// <para>
/// Normalisation: leading and trailing whitespace is trimmed from both components.
/// No casing transformation is applied — the caller is responsible for providing
/// names in the desired display casing.
/// </para>
/// <para>
/// Equality is structural: two <see cref="FullName"/> instances are equal when both
/// <c>FirstName</c> and <c>LastName</c> match using ordinal case-sensitive comparison.
/// </para>
/// </remarks>
public sealed class FullName : ValueObject
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>
    /// The maximum permitted length for each name component
    /// (<c>FirstName</c> or <c>LastName</c>) after trimming.
    /// </summary>
    public const int MaxComponentLength = 100;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    private FullName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>Gets the person's first (given) name.</summary>
    public string FirstName { get; }

    /// <summary>Gets the person's last (family) name.</summary>
    public string LastName { get; }

    /// <summary>
    /// Gets the combined full name in <c>FirstName LastName</c> format.
    /// </summary>
    public string DisplayName => $"{FirstName} {LastName}";

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="FullName"/> from the given first and last name strings.
    /// </summary>
    /// <param name="firstName">
    /// The person's first (given) name.  Leading and trailing whitespace is trimmed.
    /// Must not be <c>null</c>, empty, or whitespace.
    /// </param>
    /// <param name="lastName">
    /// The person's last (family) name.  Leading and trailing whitespace is trimmed.
    /// Must not be <c>null</c>, empty, or whitespace.
    /// </param>
    /// <returns>A validated <see cref="FullName"/> instance.</returns>
    /// <exception cref="ValidationException">
    /// Thrown when either name component is <c>null</c>, empty, whitespace, or
    /// exceeds <see cref="MaxComponentLength"/> characters after trimming.
    /// </exception>
    public static FullName Create(string firstName, string lastName)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        var trimmedFirst = Validate(firstName, nameof(firstName), errors);
        var trimmedLast = Validate(lastName, nameof(lastName), errors);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new FullName(trimmedFirst!, trimmedLast!);
    }

    // -------------------------------------------------------------------------
    // Overrides
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }

    /// <summary>
    /// Returns the full name in <c>FirstName LastName</c> format.
    /// </summary>
    public override string ToString() => DisplayName;

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates a single name component, adding any errors to the provided
    /// dictionary.  Returns the trimmed value when valid, or <c>null</c> when invalid.
    /// </summary>
    private static string? Validate(
        string? value,
        string fieldName,
        Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[fieldName] = [$"{fieldName} must not be empty."];
            return null;
        }

        var trimmed = value.Trim();

        if (trimmed.Length > MaxComponentLength)
        {
            errors[fieldName] =
                [$"{fieldName} must not exceed {MaxComponentLength} characters."];
            return null;
        }

        return trimmed;
    }
}
