using System;

namespace TopLab.Domain.Common;

/// <summary>
/// Generic strongly-typed identifier value object (ADR-0012).
/// Concrete identifiers (e.g. <c>PatientId</c>, <c>LabId</c>, <c>TestId</c>) derive
/// from this type to carry a primitive value (default <c>int</c>) in a
/// compile-time-distinct wrapper, so identifiers of different concepts cannot be
/// accidentally swapped. EF Core value converters handle the underlying storage mapping.
/// </summary>
public abstract class StronglyTypedId<TValue> : ValueObject
    where TValue : notnull, IEquatable<TValue>
{
    public TValue Value { get; }

    protected StronglyTypedId(TValue value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString() ?? string.Empty;
    }
}
