using TopLab.Domain.Attendance;
using Xunit;

namespace TopLab.Domain.Tests.Attendance;

public sealed class AttendanceCalculatorTests
{
    private static TimeOnly LocalOf(DateTime utc) => TimeOnly.FromDateTime(utc.ToLocalTime());

    [Fact]
    public void Lateness_NoConfiguredStart_ReturnsNull()
    {
        var checkIn = new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);

        Assert.Null(AttendanceCalculator.LatenessMinutes(null, checkIn));
    }

    [Fact]
    public void Lateness_ExactlyAtStart_ReturnsZero()
    {
        var checkIn = new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);
        var start = LocalOf(checkIn);

        Assert.Equal(0, AttendanceCalculator.LatenessMinutes(start, checkIn));
    }

    [Fact]
    public void Lateness_BeforeStart_ReturnsZero_NeverNegative()
    {
        var checkIn = new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);
        var start = LocalOf(checkIn).Add(TimeSpan.FromHours(1));

        Assert.Equal(0, AttendanceCalculator.LatenessMinutes(start, checkIn));
    }

    [Fact]
    public void Lateness_AfterStart_ReturnsPositiveMinutes()
    {
        var checkIn = new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);
        var start = LocalOf(checkIn).Add(TimeSpan.FromMinutes(-15));

        Assert.Equal(15, AttendanceCalculator.LatenessMinutes(start, checkIn));
    }

    [Fact]
    public void Overtime_NoConfiguredEnd_ReturnsNull()
    {
        var checkOut = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);

        Assert.Null(AttendanceCalculator.OvertimeMinutes(null, checkOut));
    }

    [Fact]
    public void Overtime_ExactlyAtEnd_ReturnsZero()
    {
        var checkOut = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);
        var end = LocalOf(checkOut);

        Assert.Equal(0, AttendanceCalculator.OvertimeMinutes(end, checkOut));
    }

    [Fact]
    public void Overtime_BeforeEnd_ReturnsZero()
    {
        var checkOut = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);
        var end = LocalOf(checkOut).Add(TimeSpan.FromHours(1));

        Assert.Equal(0, AttendanceCalculator.OvertimeMinutes(end, checkOut));
    }

    [Fact]
    public void Overtime_AfterEnd_ReturnsPositiveMinutes()
    {
        var checkOut = new DateTime(2026, 9, 15, 14, 30, 0, DateTimeKind.Utc);
        var end = LocalOf(checkOut).Add(TimeSpan.FromMinutes(-30));

        Assert.Equal(30, AttendanceCalculator.OvertimeMinutes(end, checkOut));
    }

    [Fact]
    public void WorkedMinutes_WithoutBreak_ReturnsFullSpan()
    {
        var checkIn = new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);

        Assert.Equal(480, AttendanceCalculator.WorkedMinutes(checkIn, checkOut, null, null));
    }

    [Fact]
    public void WorkedMinutes_WithBreak_SubtractsBreakSpan()
    {
        var checkIn = new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);
        var breakStart = new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc);
        var breakEnd = new DateTime(2026, 9, 15, 10, 30, 0, DateTimeKind.Utc);

        Assert.Equal(450, AttendanceCalculator.WorkedMinutes(checkIn, checkOut, breakStart, breakEnd));
    }

    [Fact]
    public void WorkedMinutes_PartialBreak_IgnoresBreak()
    {
        var checkIn = new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);
        var breakStart = new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc);

        Assert.Equal(480, AttendanceCalculator.WorkedMinutes(checkIn, checkOut, breakStart, null));
    }

    [Fact]
    public void WorkedMinutes_CrossMidnight_UsesRawInstants()
    {
        // Overnight shift: check-in before midnight, check-out after.
        // Worked minutes are computed from raw instants so durations stay correct,
        // while overtime is measured against the same calendar day's WorkEndTime.
        var checkIn = new DateTime(2026, 9, 15, 20, 0, 0, DateTimeKind.Utc);
        var checkOut = new DateTime(2026, 9, 16, 4, 0, 0, DateTimeKind.Utc);

        Assert.Equal(480, AttendanceCalculator.WorkedMinutes(checkIn, checkOut, null, null));

        var sameDayEnd = LocalOf(checkOut);
        var overtime = AttendanceCalculator.OvertimeMinutes(sameDayEnd, checkOut);
        Assert.Equal(0, overtime);
    }
}
