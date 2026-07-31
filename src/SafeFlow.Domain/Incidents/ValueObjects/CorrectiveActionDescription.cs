using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.Incidents.ValueObjects;

/// <summary>
/// Immutable Value Object representing a Corrective Action description.
/// </summary>
public sealed class CorrectiveActionDescription : ValueObject
{
    public const int MaxLength = 2000;

    private CorrectiveActionDescription(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static CorrectiveActionDescription Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        string trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"Corrective action description must not exceed {MaxLength} characters.", nameof(value));
        }

        return new CorrectiveActionDescription(trimmed);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
