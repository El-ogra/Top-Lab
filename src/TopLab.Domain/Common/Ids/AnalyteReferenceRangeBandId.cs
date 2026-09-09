using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for AnalyteReferenceRangeBandId.</summary>
public sealed class AnalyteReferenceRangeBandId : StronglyTypedId<int>
{
    private AnalyteReferenceRangeBandId(int value) : base(value)
    {
    }

    public static AnalyteReferenceRangeBandId Create(int value) => new(value);
}