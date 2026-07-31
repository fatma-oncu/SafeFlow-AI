using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.RiskAssessments.ValueObjects;

/// <summary>
/// Value Object encapsulating a validated hazard description text.
/// </summary>
public sealed class HazardDescription : ValueObject
{
    public const int MaxLength = 500;

    private HazardDescription()
    {
        Value = string.Empty;
    }

    private HazardDescription(string value)
    {
        Value = value;
    }

    /// <summary>Gets the hazard description text.</summary>
    public string Value { get; private set; }

    /// <summary>
    /// Factory method to create a validated <see cref="HazardDescription"/>.
    /// </summary>
    public static HazardDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(HazardDescription)] = ["Hazard description must not be empty."]
            });

        string trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(HazardDescription)] = [$"Hazard description must not exceed {MaxLength} characters."]
            });

        return new HazardDescription(trimmed);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
