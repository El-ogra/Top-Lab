using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for SentOutSamplePaymentId.</summary>
public sealed class SentOutSamplePaymentId : StronglyTypedId<int>
{
    private SentOutSamplePaymentId(int value) : base(value)
    {
    }

    public static SentOutSamplePaymentId Create(int value) => new(value);
}
