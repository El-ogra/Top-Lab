using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for AttendanceRecordId.</summary>
public sealed class AttendanceRecordId : StronglyTypedId<int>
{
    private AttendanceRecordId(int value) : base(value)
    {
    }

    public static AttendanceRecordId Create(int value) => new(value);
}
