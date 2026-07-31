using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.Incidents.ValueObjects;

/// <summary>
/// Immutable Value Object representing an Incident description.
/// </summary>
public sealed class IncidentDescription : ValueObject
{
    public const int MaxLength = 4000;

    private IncidentDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static IncidentDescription Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        string trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Incident description must not exceed {MaxLength} characters.", nameof(value));
        }

        return new IncidentDescription(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
