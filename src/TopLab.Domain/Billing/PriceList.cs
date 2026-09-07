using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Billing;

public sealed class PriceList : Entity<PriceListId>
{
    public string Name { get; private set; } = default!;

    private readonly List<PriceListItem> _items = [];
    public IReadOnlyCollection<PriceListItem> Items => _items.AsReadOnly();

    private PriceList()
    {
    }

    private PriceList(PriceListId id, string name)
        : base(id)
    {
        Name = name;
    }

    public static PriceList Create(PriceListId id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return new PriceList(id, name.Trim());
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        Name = name.Trim();
    }

    public bool ContainsTest(TestId testId)
    {
        return _items.Any(i => i.TestId == testId);
    }

    public void AddItem(TestId testId, decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentException("Price must be >= 0.", nameof(price));
        }

        if (_items.Any(i => i.TestId == testId))
        {
            throw new ArgumentException("Test already exists in the price list.", nameof(testId));
        }

        _items.Add(new PriceListItem(Id, testId, price));
    }

    public void SetItemPrice(TestId testId, decimal price)
    {
        var existing = _items.FirstOrDefault(i => i.TestId == testId);

        if (existing is not null)
        {
            existing.UpdatePrice(price);
            return;
        }

        AddItem(testId, price);
    }

    public void RemoveItem(TestId testId)
    {
        var item = _items.FirstOrDefault(i => i.TestId == testId)
            ?? throw new ArgumentException("Test is not in the price list.", nameof(testId));

        _items.Remove(item);
    }
}
