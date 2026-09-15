using TopLab.Domain.Attendance;

namespace TopLab.Application.Features.Attendance.Common;

public sealed record AttendanceRecordDto(
    int Id,
    int UserId,
    string UserName,
    DateTime CheckInAtUtc,
    DateTime? BreakStartAtUtc,
    DateTime? BreakEndAtUtc,
    DateTime? CheckOutAtUtc,
    int? LatenessMinutes,
    int? OvertimeMinutes,
    int? WorkedMinutes)
{
    public static AttendanceRecordDto FromRecord(AttendanceRecord record, string userName)
    {
        int? worked = record.CheckOutAtUtc is null
            ? null
            : AttendanceCalculator.WorkedMinutes(
                record.CheckInAtUtc,
                record.CheckOutAtUtc.Value,
                record.BreakStartAtUtc,
                record.BreakEndAtUtc);

        return new AttendanceRecordDto(
            record.Id.Value,
            record.UserId.Value,
            userName,
            record.CheckInAtUtc,
            record.BreakStartAtUtc,
            record.BreakEndAtUtc,
            record.CheckOutAtUtc,
            record.LatenessMinutes,
            record.OvertimeMinutes,
            worked);
    }
}

public sealed record UserAttendanceSummaryDto(
    int UserId,
    string UserName,
    DateOnly From,
    DateOnly To,
    int DaysPresent,
    int RecordsCount,
    int TotalWorkedMinutes,
    int TotalLatenessMinutes,
    int TotalOvertimeMinutes);
