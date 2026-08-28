using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for WorkGroupLogId.</summary>
public sealed class WorkGroupLogId : StronglyTypedId<int>
{
    private WorkGroupLogId(int value) : base(value)
    {
    }

    public static WorkGroupLogId Create(int value) => new(value);
}
