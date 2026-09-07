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
        if (price < 0)
        {
            throw new ArgumentException("Price must be >= 0.", nameof(price));
        }

        PriceListId = priceListId;
        TestId = testId;
        Price = price;
    }

    internal void UpdatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentException("Price must be >= 0.", nameof(price));
        }

        Price = price;
    }
}
