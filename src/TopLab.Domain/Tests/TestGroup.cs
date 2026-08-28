using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

public sealed class TestGroup : Entity<TestGroupId>
{
    public string Name { get; private set; } = default!;

    private TestGroup()
    {
    }

    private TestGroup(TestGroupId id, string name)
        : base(id)
    {
        Name = name;
    }

    public static TestGroup Create(TestGroupId id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return new TestGroup(id, name.Trim());
    }
}
