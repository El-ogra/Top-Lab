using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

public sealed class TestGroup : Entity<TestGroupId>
{
    public string Name { get; private set; } = default!;

    public bool IsActive { get; private set; }

    private TestGroup()
    {
    }

    private TestGroup(TestGroupId id, string name, bool isActive)
        : base(id)
    {
        Name = name;
        IsActive = isActive;
    }

    public static TestGroup Create(TestGroupId id, string name, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return new TestGroup(id, name.Trim(), isActive);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        Name = name.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Reactivate()
    {
        IsActive = true;
    }
}