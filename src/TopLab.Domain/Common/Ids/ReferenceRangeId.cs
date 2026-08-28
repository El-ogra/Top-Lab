using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for ReferenceRangeId.</summary>
public sealed class ReferenceRangeId : StronglyTypedId<int>
{
    private ReferenceRangeId(int value) : base(value)
    {
    }

    public static ReferenceRangeId Create(int value) => new(value);
}
