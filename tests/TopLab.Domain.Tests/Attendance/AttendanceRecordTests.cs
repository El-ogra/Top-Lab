using TopLab.Domain.Attendance;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Attendance;

public sealed class AttendanceRecordTests
{
    private static AttendanceRecord CreateOpen(DateTime? checkInAtUtc = null)
    {
        return AttendanceRecord.Create(
            AttendanceRecordId.Create(0),
            UserId.Create(1),
            checkInAtUtc ?? new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Create_SetsCheckInAndLateness()
    {
        var checkIn = new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);

        var record = AttendanceRecord.Create(
            AttendanceRecordId.Create(0),
            UserId.Create(7),
            checkIn,
            latenessMinutes: 5);

        Assert.Equal(7, record.UserId.Value);
        Assert.Equal(checkIn, record.CheckInAtUtc);
        Assert.Equal(5, record.LatenessMinutes);
        Assert.Null(record.BreakStartAtUtc);
        Assert.Null(record.BreakEndAtUtc);
        Assert.Null(record.CheckOutAtUtc);
    }

    [Fact]
    public void StartBreak_HappyPath_SetsBreakStart()
    {
        var record = CreateOpen();
        var at = new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc);

        record.StartBreak(at);

        Assert.Equal(at, record.BreakStartAtUtc);
    }

    [Fact]
    public void StartBreak_DoubleStart_Throws()
    {
        var record = CreateOpen();
        record.StartBreak(new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc));

        var ex = Assert.Throws<InvalidOperationException>(
            () => record.StartBreak(new DateTime(2026, 9, 15, 10, 5, 0, DateTimeKind.Utc)));
        Assert.Equal("Break is already started.", ex.Message);
    }

    [Fact]
    public void StartBreak_AfterCheckOut_Throws()
    {
        var record = CreateOpen();
        record.CheckOut(new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc), overtimeMinutes: 0);

        var ex = Assert.Throws<InvalidOperationException>(
            () => record.StartBreak(new DateTime(2026, 9, 15, 15, 0, 0, DateTimeKind.Utc)));
        Assert.Equal("Already checked out.", ex.Message);
    }

    [Fact]
    public void EndBreak_HappyPath_SetsBreakEnd()
    {
        var record = CreateOpen();
        record.StartBreak(new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc));
        var end = new DateTime(2026, 9, 15, 10, 30, 0, DateTimeKind.Utc);

        record.EndBreak(end);

        Assert.Equal(end, record.BreakEndAtUtc);
    }

    [Fact]
    public void EndBreak_WithoutStart_Throws()
    {
        var record = CreateOpen();

        var ex = Assert.Throws<InvalidOperationException>(
            () => record.EndBreak(new DateTime(2026, 9, 15, 10, 30, 0, DateTimeKind.Utc)));
        Assert.Equal("No open break to end.", ex.Message);
    }

    [Fact]
    public void EndBreak_DoubleEnd_Throws()
    {
        var record = CreateOpen();
        record.StartBreak(new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc));
        record.EndBreak(new DateTime(2026, 9, 15, 10, 30, 0, DateTimeKind.Utc));

        var ex = Assert.Throws<InvalidOperationException>(
            () => record.EndBreak(new DateTime(2026, 9, 15, 11, 0, 0, DateTimeKind.Utc)));
        Assert.Equal("No open break to end.", ex.Message);
    }

    [Fact]
    public void CheckOut_HappyPath_SetsCheckOutAndOvertime()
    {
        var record = CreateOpen();
        var at = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);

        record.CheckOut(at, overtimeMinutes: 12);

        Assert.Equal(at, record.CheckOutAtUtc);
        Assert.Equal(12, record.OvertimeMinutes);
    }

    [Fact]
    public void CheckOut_WithOpenBreak_Throws()
    {
        var record = CreateOpen();
        record.StartBreak(new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc));

        var ex = Assert.Throws<InvalidOperationException>(
            () => record.CheckOut(new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc), overtimeMinutes: 0));
        Assert.Equal("Cannot check out with an open break.", ex.Message);
    }

    [Fact]
    public void CheckOut_DoubleCheckOut_Throws()
    {
        var record = CreateOpen();
        record.CheckOut(new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc), overtimeMinutes: 0);

        var ex = Assert.Throws<InvalidOperationException>(
            () => record.CheckOut(new DateTime(2026, 9, 15, 15, 0, 0, DateTimeKind.Utc), overtimeMinutes: 0));
        Assert.Equal("Already checked out.", ex.Message);
    }

    [Fact]
    public void FullLifecycle_HappyPath()
    {
        var record = CreateOpen(new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc));

        record.StartBreak(new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc));
        record.EndBreak(new DateTime(2026, 9, 15, 10, 30, 0, DateTimeKind.Utc));
        record.CheckOut(new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc), overtimeMinutes: 0);

        Assert.NotNull(record.BreakStartAtUtc);
        Assert.NotNull(record.BreakEndAtUtc);
        Assert.NotNull(record.CheckOutAtUtc);
    }
}
