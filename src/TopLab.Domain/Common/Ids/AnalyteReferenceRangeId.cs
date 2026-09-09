using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for AnalyteReferenceRangeId.</summary>
public sealed class AnalyteReferenceRangeId : StronglyTypedId<int>
{
    private AnalyteReferenceRangeId(int value) : base(value)
    {
    }

    public static AnalyteReferenceRangeId Create(int value) => new(value);
}