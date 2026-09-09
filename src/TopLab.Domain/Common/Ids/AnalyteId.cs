using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for AnalyteId.</summary>
public sealed class AnalyteId : StronglyTypedId<int>
{
    private AnalyteId(int value) : base(value)
    {
    }

    public static AnalyteId Create(int value) => new(value);
}