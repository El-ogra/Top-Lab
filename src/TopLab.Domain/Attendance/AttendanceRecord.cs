using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Attendance;

public sealed class AttendanceRecord : Entity<AttendanceRecordId>
{
    public UserId UserId { get; private set; } = default!;

    public DateTime CheckInAtUtc { get; private set; }

    public DateTime? BreakStartAtUtc { get; private set; }

    public DateTime? BreakEndAtUtc { get; private set; }

    public DateTime? CheckOutAtUtc { get; private set; }

    public int? OvertimeMinutes { get; private set; }

    public int? LatenessMinutes { get; private set; }

    private AttendanceRecord()
    {
    }

    private AttendanceRecord(AttendanceRecordId id, UserId userId, DateTime checkInAtUtc, int? latenessMinutes)
        : base(id)
    {
        UserId = userId;
        CheckInAtUtc = checkInAtUtc;
        LatenessMinutes = latenessMinutes;
    }

    public static AttendanceRecord Create(AttendanceRecordId id, UserId userId, DateTime checkInAtUtc, int? latenessMinutes = null)
    {
        return new AttendanceRecord(id, userId, checkInAtUtc, latenessMinutes);
    }

    public void StartBreak(DateTime atUtc)
    {
        BreakStartAtUtc = atUtc;
    }

    public void EndBreak(DateTime atUtc)
    {
        BreakEndAtUtc = atUtc;
    }

    public void CheckOut(DateTime atUtc, int? overtimeMinutes)
    {
        CheckOutAtUtc = atUtc;
        OvertimeMinutes = overtimeMinutes;
    }
}
