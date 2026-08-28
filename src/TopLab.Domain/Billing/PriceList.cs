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
}
