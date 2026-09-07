using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

public sealed class CustomGroup : Entity<CustomGroupId>
{
    public string Name { get; private set; } = default!;

    private readonly List<CustomGroupItem> _items = [];
    public IReadOnlyCollection<CustomGroupItem> Items => _items.AsReadOnly();

    private CustomGroup()
    {
    }

    private CustomGroup(CustomGroupId id, string name)
        : base(id)
    {
        Name = name;
    }

    public static CustomGroup Create(CustomGroupId id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return new CustomGroup(id, name.Trim());
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
            throw new ArgumentException("Test already exists in the custom group.", nameof(testId));
        }

        _items.Add(new CustomGroupItem(Id, testId, price));
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
            ?? throw new ArgumentException("Test is not in the custom group.", nameof(testId));

        _items.Remove(item);
    }
}
