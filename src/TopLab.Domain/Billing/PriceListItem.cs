using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Billing;

/// <summary>Composite PK: PriceListId + TestId.</summary>
public sealed class PriceListItem
{
    public PriceListId PriceListId { get; private set; } = default!;

    public TestId TestId { get; private set; } = default!;

    public decimal Price { get; private set; }

    private PriceListItem()
    {
    }

    public PriceListItem(PriceListId priceListId, TestId testId, decimal price)
    {
        PriceListId = priceListId;
        TestId = testId;
        Price = price;
    }
}
