using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for PriceListId.</summary>
public sealed class PriceListId : StronglyTypedId<int>
{
    private PriceListId(int value) : base(value)
    {
    }

    public static PriceListId Create(int value) => new(value);
}
