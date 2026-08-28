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
}
