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
}
