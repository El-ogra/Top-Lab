using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

/// <summary>Composite PK: CustomGroupId + TestId.</summary>
public sealed class CustomGroupItem
{
    public CustomGroupId CustomGroupId { get; private set; } = default!;

    public TestId TestId { get; private set; } = default!;

    public decimal Price { get; private set; }

    private CustomGroupItem()
    {
    }

    public CustomGroupItem(CustomGroupId customGroupId, TestId testId, decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentException("Price must be >= 0.", nameof(price));
        }

        CustomGroupId = customGroupId;
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
