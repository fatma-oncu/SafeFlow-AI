using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.Employees.ValueObjects;

/// <summary>
/// Represents a unique, formatted employee identifier string (e.g., <c>"EMP-2026-0001"</c>).
/// </summary>
public sealed class EmployeeNumber : ValueObject
{
    public const int MaxLength = 50;

    private EmployeeNumber()
    {
        Value = string.Empty;
    }

    private EmployeeNumber(string value)
    {
        Value = value;
    }

    /// <summary>Gets the raw string value of the employee number.</summary>
    public string Value { get; private set; }

    /// <summary>
    /// Creates a validated <see cref="EmployeeNumber"/>.
    /// </summary>
    public static EmployeeNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(EmployeeNumber)] = ["Employee number must not be empty."]
            });

        string trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(EmployeeNumber)] = [$"Employee number must not exceed {MaxLength} characters."]
            });

        return new EmployeeNumber(trimmed);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    /// <inheritdoc/>
    public override string ToString() => Value;
}
