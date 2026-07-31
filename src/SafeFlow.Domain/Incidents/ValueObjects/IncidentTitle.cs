using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.Incidents.ValueObjects;

/// <summary>
/// Immutable Value Object representing an Incident title.
/// </summary>
public sealed class IncidentTitle : ValueObject
{
    public const int MaxLength = 200;

    private IncidentTitle(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static IncidentTitle Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        string trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Incident title must not exceed {MaxLength} characters.", nameof(value));
        }

        return new IncidentTitle(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
