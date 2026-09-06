using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

public sealed class WorkGroupLog : Entity<WorkGroupLogId>
{
    public string Name { get; private set; } = default!;

    private readonly List<WorkGroupLogItem> _items = [];
    public IReadOnlyCollection<WorkGroupLogItem> Items => _items.AsReadOnly();

    private WorkGroupLog()
    {
    }

    private WorkGroupLog(WorkGroupLogId id, string name)
        : base(id)
    {
        Name = name;
    }

    public static WorkGroupLog Create(WorkGroupLogId id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return new WorkGroupLog(id, name.Trim());
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

    public void AddItem(TestId testId)
    {
        if (_items.Any(i => i.TestId == testId))
        {
            throw new ArgumentException("Test already exists in the work group log.", nameof(testId));
        }

        _items.Add(WorkGroupLogItem.Create(Id, testId));
    }

    public void RemoveItem(TestId testId)
    {
        var item = _items.FirstOrDefault(i => i.TestId == testId)
            ?? throw new ArgumentException("Test is not in the work group log.", nameof(testId));

        _items.Remove(item);
    }

    public void ClearItems()
    {
        _items.Clear();
    }
}