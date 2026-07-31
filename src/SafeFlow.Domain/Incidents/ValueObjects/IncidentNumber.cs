using System.Text.RegularExpressions;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.Incidents.ValueObjects;

/// <summary>
/// Immutable Value Object representing a formatted incident number (e.g. INC-2026-000001).
/// </summary>
public partial class IncidentNumber : ValueObject
{
    private static readonly Regex Pattern = NumberRegex();

    private IncidentNumber(string value)
    {
        Value = value;
    }

    /// <summary>Gets the string value of the incident number.</summary>
    public string Value { get; }

    /// <summary>
    /// Factory method to create an <see cref="IncidentNumber"/>.
    /// </summary>
    public static IncidentNumber Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        string trimmed = value.Trim().ToUpperInvariant();
        if (!Pattern.IsMatch(trimmed))
        {
            throw new ArgumentException(
                $"Incident number '{value}' is invalid. Must match format INC-YYYY-XXXXXX.",
                nameof(value));
        }

        return new IncidentNumber(trimmed);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value;

    [GeneratedRegex(@"^INC-\d{4}-\d{6}$", RegexOptions.Compiled)]
    private static partial Regex NumberRegex();
}
