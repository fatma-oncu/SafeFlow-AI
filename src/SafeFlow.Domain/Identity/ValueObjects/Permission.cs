using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.Identity.ValueObjects;

/// <summary>
/// Represents a single, discrete authorisation capability assigned to a
/// <c>Role</c> within the SafeFlow permission model.
/// </summary>
/// <remarks>
/// <para>
/// A permission is identified by two components:
/// <list type="bullet">
///   <item>
///     <term><c>Module</c></term>
///     <description>
///     The bounded-context or feature area the permission protects
///     (e.g., <c>"Users"</c>, <c>"Incidents"</c>, <c>"Training"</c>).
///     </description>
///   </item>
///   <item>
///     <term><c>Action</c></term>
///     <description>
///     The specific operation permitted within the module
///     (e.g., <c>"Read"</c>, <c>"Write"</c>, <c>"Delete"</c>, <c>"Admin"</c>).
///     </description>
///   </item>
/// </list>
/// </para>
/// <para>
/// The <c>CanonicalName</c> (<c>Module.Action</c>) is the primary string used
/// in JWT claims and API policy names, keeping the authorization infrastructure
/// decoupled from the domain model.
/// </para>
/// <para>
/// Equality is case-insensitive: <c>"users.read"</c> equals <c>"Users.Read"</c>.
/// </para>
/// </remarks>
public sealed class Permission : ValueObject
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    /// <summary>Maximum permitted length for the <c>Module</c> component.</summary>
    public const int MaxModuleLength = 100;

    /// <summary>Maximum permitted length for the <c>Action</c> component.</summary>
    public const int MaxActionLength = 100;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    private Permission(string module, string action)
    {
        Module = module;
        Action = action;
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the module (bounded-context or feature area) component of this permission
    /// (e.g., <c>"Users"</c>, <c>"Incidents"</c>).
    /// </summary>
    public string Module { get; }

    /// <summary>
    /// Gets the action component of this permission
    /// (e.g., <c>"Read"</c>, <c>"Write"</c>, <c>"Delete"</c>).
    /// </summary>
    public string Action { get; }

    /// <summary>
    /// Gets the canonical dot-separated representation of this permission
    /// (e.g., <c>"Users.Read"</c>, <c>"Incidents.Delete"</c>).
    /// Used as the claim value in JWT tokens and as the ASP.NET Core policy name.
    /// </summary>
    public string CanonicalName => $"{Module}.{Action}";

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new <see cref="Permission"/> from the given module and action strings.
    /// </summary>
    /// <param name="module">
    /// The bounded-context or feature area (e.g., <c>"Users"</c>).
    /// Must not be <c>null</c> or whitespace.
    /// Must not exceed <see cref="MaxModuleLength"/> characters.
    /// </param>
    /// <param name="action">
    /// The permitted operation (e.g., <c>"Read"</c>).
    /// Must not be <c>null</c> or whitespace.
    /// Must not exceed <see cref="MaxActionLength"/> characters.
    /// </param>
    /// <returns>A validated, immutable <see cref="Permission"/> instance.</returns>
    /// <exception cref="ValidationException">
    /// Thrown when either component is <c>null</c>, whitespace, or exceeds its
    /// maximum permitted length.
    /// </exception>
    public static Permission Create(string module, string action)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        var trimModule = ValidateComponent(module, nameof(module), MaxModuleLength, errors);
        var trimAction = ValidateComponent(action, nameof(action), MaxActionLength, errors);

        if (errors.Count > 0)
            throw new ValidationException(errors);

        return new Permission(trimModule!, trimAction!);
    }

    // -------------------------------------------------------------------------
    // Overrides
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        // Case-insensitive equality: "users.read" == "Users.Read"
        yield return Module.ToUpperInvariant();
        yield return Action.ToUpperInvariant();
    }

    /// <summary>Returns the canonical name (e.g., <c>"Users.Read"</c>).</summary>
    public override string ToString() => CanonicalName;

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static string? ValidateComponent(
        string? value,
        string fieldName,
        int maxLength,
        Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[fieldName] = [$"Permission {fieldName} must not be empty."];
            return null;
        }

        var trimmed = value.Trim();

        if (trimmed.Length > maxLength)
        {
            errors[fieldName] = [$"Permission {fieldName} must not exceed {maxLength} characters."];
            return null;
        }

        return trimmed;
    }
}
