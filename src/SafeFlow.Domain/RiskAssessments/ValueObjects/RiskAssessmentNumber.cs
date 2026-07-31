using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.RiskAssessments.ValueObjects;

/// <summary>
/// Value Object representing a unique, formatted risk assessment identifier (e.g., <c>"RA-2026-000001"</c>).
/// </summary>
public sealed class RiskAssessmentNumber : ValueObject
{
    public const int MaxLength = 50;

    private RiskAssessmentNumber()
    {
        Value = string.Empty;
    }

    private RiskAssessmentNumber(string value)
    {
        Value = value;
    }

    /// <summary>Gets the raw string value of the assessment number.</summary>
    public string Value { get; private set; }

    /// <summary>
    /// Factory method to create a validated <see cref="RiskAssessmentNumber"/>.
    /// </summary>
    public static RiskAssessmentNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(RiskAssessmentNumber)] = ["Risk assessment number must not be empty."]
            });

        string trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(RiskAssessmentNumber)] = [$"Risk assessment number must not exceed {MaxLength} characters."]
            });

        return new RiskAssessmentNumber(trimmed);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
