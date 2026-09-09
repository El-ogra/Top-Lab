using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for ProfileAnalyteId.</summary>
public sealed class ProfileAnalyteId : StronglyTypedId<int>
{
    private ProfileAnalyteId(int value) : base(value)
    {
    }

    public static ProfileAnalyteId Create(int value) => new(value);
}