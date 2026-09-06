using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

public sealed class WorkGroupLogItem
{
    public WorkGroupLogId WorkGroupLogId { get; private set; } = default!;

    public TestId TestId { get; private set; } = default!;

    public WorkGroupLogItem()
    {
    }

    private WorkGroupLogItem(WorkGroupLogId workGroupLogId, TestId testId)
    {
        WorkGroupLogId = workGroupLogId;
        TestId = testId;
    }

    public static WorkGroupLogItem Create(WorkGroupLogId workGroupLogId, TestId testId)
    {
        if (workGroupLogId is null)
        {
            throw new ArgumentNullException(nameof(workGroupLogId));
        }

        if (testId is null)
        {
            throw new ArgumentNullException(nameof(testId));
        }

        return new WorkGroupLogItem(workGroupLogId, testId);
    }
}