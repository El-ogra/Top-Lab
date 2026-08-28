using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for SentOutSampleId.</summary>
public sealed class SentOutSampleId : StronglyTypedId<int>
{
    private SentOutSampleId(int value) : base(value)
    {
    }

    public static SentOutSampleId Create(int value) => new(value);
}
