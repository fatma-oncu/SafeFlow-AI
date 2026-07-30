using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.Employees.ValueObjects;

/// <summary>
/// Value object representing an employee's job title.
/// </summary>
public sealed class JobTitle : ValueObject
{
    public const int MaxLength = 100;

    private JobTitle()
    {
        Value = string.Empty;
    }

    private JobTitle(string value)
    {
        Value = value;
    }

    /// <summary>Gets the job title text.</summary>
    public string Value { get; private set; }

    /// <summary>Creates a validated <see cref="JobTitle"/>.</summary>
    public static JobTitle Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(JobTitle)] = ["Job title must not be empty."]
            });

        string trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(JobTitle)] = [$"Job title must not exceed {MaxLength} characters."]
            });

        return new JobTitle(trimmed);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
