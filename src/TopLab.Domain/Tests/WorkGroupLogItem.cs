using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

public sealed class WorkGroupLogItem
{
    public WorkGroupLogId WorkGroupLogId { get; private set; } = default!;

    public TestId TestId { get; private set; } = default!;

    private WorkGroupLogItem()
    {
    }

    public WorkGroupLogItem(WorkGroupLogId workGroupLogId, TestId testId)
    {
        WorkGroupLogId = workGroupLogId;
        TestId = testId;
    }
}
