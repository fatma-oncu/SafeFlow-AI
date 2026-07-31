using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.Incidents.ValueObjects;

/// <summary>
/// Immutable Value Object representing an Incident physical location.
/// </summary>
public sealed class IncidentLocation : ValueObject
{
    public const int MaxLength = 500;

    private IncidentLocation(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static IncidentLocation Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        string trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Incident location must not exceed {MaxLength} characters.", nameof(value));
        }

        return new IncidentLocation(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
