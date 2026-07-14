namespace SafeFlow.SharedKernel.ValueObjects;

/// <summary>
/// Abstract base class for all value objects in the SafeFlow domain model.
/// </summary>
/// <remarks>
/// <para>
/// A <em>value object</em> is an immutable domain concept that is defined entirely
/// by the values of its attributes. Two value objects are equal when all their
/// constituent attribute values are equal — they carry no identity of their own.
/// </para>
/// <para>
/// Concrete implementations must override <see cref="GetEqualityComponents"/> and
/// yield every attribute that participates in equality. The base class derives
/// <see cref="Equals(object?)"/> and <see cref="GetHashCode"/> from those components,
/// so subclasses never need to override those methods manually.
/// </para>
/// <para>
/// Value objects must be immutable after construction. Properties should be
/// <c>init</c>-only or read-only and all mutation must produce a new instance.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class Email : ValueObject
/// {
///     public string Value { get; }
///
///     public Email(string value)
///     {
///         if (string.IsNullOrWhiteSpace(value))
///             throw new ArgumentException("Email cannot be empty.", nameof(value));
///         Value = value.Trim().ToLowerInvariant();
///     }
///
///     protected override IEnumerable&lt;object?&gt; GetEqualityComponents()
///     {
///         yield return Value;
///     }
/// }
/// </code>
/// </example>
public abstract class ValueObject
{
    /// <summary>
    /// Returns the sequence of values that define the identity of this value object.
    /// Every property that participates in equality must be yielded here.
    /// </summary>
    /// <returns>
    /// An ordered sequence of the attribute values. The sequence must be
    /// deterministic and complete.
    /// </returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Determines whether this value object is equal to another object by
    /// comparing the sequences returned by <see cref="GetEqualityComponents"/>.
    /// </summary>
    /// <param name="obj">The object to compare with this value object.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="obj"/> is the same runtime type and all
    /// equality components match; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        return GetEqualityComponents()
            .SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }

    /// <summary>
    /// Returns a hash code derived from all equality components.
    /// </summary>
    /// <returns>A composite hash code for this value object.</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(
                new HashCode(),
                (hash, component) =>
                {
                    hash.Add(component);
                    return hash;
                })
            .ToHashCode();
    }

    /// <summary>Returns <c>true</c> when both value objects are equal.</summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    /// <summary>Returns <c>true</c> when the two value objects are not equal.</summary>
    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
