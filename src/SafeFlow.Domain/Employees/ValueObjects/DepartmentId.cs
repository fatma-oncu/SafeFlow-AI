using SafeFlow.SharedKernel.Exceptions;
using SafeFlow.SharedKernel.ValueObjects;

namespace SafeFlow.Domain.Employees.ValueObjects;

/// <summary>
/// Value object representing a Department identity.
/// </summary>
public sealed class DepartmentId : ValueObject
{
    private DepartmentId()
    {
        Value = Guid.Empty;
    }

    private DepartmentId(Guid value)
    {
        Value = value;
    }

    /// <summary>Gets the underlying <see cref="Guid"/> value.</summary>
    public Guid Value { get; private set; }

    /// <summary>Creates a new <see cref="DepartmentId"/> with a new Guid.</summary>
    public static DepartmentId CreateUnique() => new(Guid.NewGuid());

    /// <summary>Creates a <see cref="DepartmentId"/> from an existing Guid.</summary>
    public static DepartmentId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(DepartmentId)] = ["Department identifier must not be empty."]
            });

        return new DepartmentId(value);
    }

    /// <inheritdoc/>
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value.ToString();
}
