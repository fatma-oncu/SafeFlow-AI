using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.RiskAssessments.ValueObjects;

/// <summary>
/// Value Object encapsulating a validated mitigation control description.
/// </summary>
public sealed class ControlDescription : ValueObject
{
    public const int MaxLength = 500;

    private ControlDescription()
    {
        Value = string.Empty;
    }

    private ControlDescription(string value)
    {
        Value = value;
    }

    /// <summary>Gets the control measure description text.</summary>
    public string Value { get; private set; }

    /// <summary>
    /// Factory method to create a validated <see cref="ControlDescription"/>.
    /// </summary>
    public static ControlDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(ControlDescription)] = ["Control description must not be empty."]
            });

        string trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(ControlDescription)] = [$"Control description must not exceed {MaxLength} characters."]
            });

        return new ControlDescription(trimmed);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
