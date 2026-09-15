using TopLab.Application.Features.Attendance.Queries.GetUserAttendanceSummary;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Attendance;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.Attendance;

public sealed class GetUserAttendanceSummaryQueryHandlerTests
{
    private static (FakeApplicationDbContext db, FakeCurrentUserService user, FakeDateTimeProvider time) NewCtx(bool absolute = true)
    {
        var ctx = (db: new FakeApplicationDbContext(), user: new FakeCurrentUserService(), time: new FakeDateTimeProvider());
        ctx.user.IsAbsolutePermission = absolute;
        return ctx;
    }

    private static void SeedUser(FakeApplicationDbContext db, int id)
    {
        db.Users.Add(User.Create(UserId.Create(id), $"user{id}", "pwdhash", "internalhash"));
    }

    private static AttendanceRecord SeedRecord(
        FakeApplicationDbContext db,
        int userId,
        DateTime checkIn,
        int? lateness = null,
        DateTime? breakStart = null,
        DateTime? breakEnd = null,
        DateTime? checkOut = null,
        int? overtime = null)
    {
        var record = AttendanceRecord.Create(
            AttendanceRecordId.Create(0),
            UserId.Create(userId),
            checkIn,
            lateness);
        db.AttendanceRecords.Add(record);
        if (breakStart.HasValue)
        {
            record.StartBreak(breakStart.Value);
        }

        if (breakEnd.HasValue)
        {
            record.EndBreak(breakEnd.Value);
        }

        if (checkOut.HasValue)
        {
            record.CheckOut(checkOut.Value, overtime);
        }

        return record;
    }

    [Fact]
    public async Task NonAbsolute_Forbidden_Verbatim()
    {
        var (db, user, time) = NewCtx(absolute: false);
        SeedUser(db, 1);

        var handler = new GetUserAttendanceSummaryQueryHandler(db, user, time);
        var result = await handler.Handle(
            new GetUserAttendanceSummaryQuery(1, new DateOnly(2026, 9, 15), new DateOnly(2026, 9, 15)),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", result.Error!.Message);
    }

    [Fact]
    public async Task UnknownUser_NotFound()
    {
        var (db, user, time) = NewCtx();

        var handler = new GetUserAttendanceSummaryQueryHandler(db, user, time);
        var result = await handler.Handle(
            new GetUserAttendanceSummaryQuery(9, new DateOnly(2026, 9, 15), new DateOnly(2026, 9, 15)),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المستخدم غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task Totals_MatchCalculator_NumberForNumber()
    {
        var (db, user, time) = NewCtx();
        SeedUser(db, 1);

        var r1 = SeedRecord(
            db, 1,
            checkIn: new DateTime(2026, 9, 15, 6, 0, 0),
            lateness: 5,
            breakStart: new DateTime(2026, 9, 15, 10, 0, 0),
            breakEnd: new DateTime(2026, 9, 15, 10, 30, 0),
            checkOut: new DateTime(2026, 9, 15, 14, 0, 0),
            overtime: 10);
        var r2 = SeedRecord(
            db, 1,
            checkIn: new DateTime(2026, 9, 15, 6, 5, 0),
            lateness: 0,
            checkOut: new DateTime(2026, 9, 16, 14, 5, 0),
            overtime: null);
        var r3 = SeedRecord(
            db, 1,
            checkIn: new DateTime(2026, 9, 16, 6, 0, 0),
            lateness: 2);
        SeedRecord(
            db, 1,
            checkIn: new DateTime(2026, 9, 20, 6, 0, 0),
            lateness: 100,
            checkOut: new DateTime(2026, 9, 20, 14, 0, 0),
            overtime: 100);

        var handler = new GetUserAttendanceSummaryQueryHandler(db, user, time);
        var result = await handler.Handle(
            new GetUserAttendanceSummaryQuery(1, new DateOnly(2026, 9, 15), new DateOnly(2026, 9, 16)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var summary = result.Value!;
        Assert.Equal(1, summary.UserId);
        Assert.Equal("user1", summary.UserName);
        Assert.Equal(new DateOnly(2026, 9, 15), summary.From);
        Assert.Equal(new DateOnly(2026, 9, 16), summary.To);
        Assert.Equal(3, summary.RecordsCount);
        Assert.Equal(2, summary.DaysPresent);

        var expectedWorked =
            AttendanceCalculator.WorkedMinutes(r1.CheckInAtUtc, r1.CheckOutAtUtc!.Value, r1.BreakStartAtUtc, r1.BreakEndAtUtc) +
            AttendanceCalculator.WorkedMinutes(r2.CheckInAtUtc, r2.CheckOutAtUtc!.Value, r2.BreakStartAtUtc, r2.BreakEndAtUtc);
        Assert.Equal(expectedWorked, summary.TotalWorkedMinutes);
        Assert.Equal((r1.LatenessMinutes ?? 0) + (r2.LatenessMinutes ?? 0) + (r3.LatenessMinutes ?? 0), summary.TotalLatenessMinutes);
        Assert.Equal((r1.OvertimeMinutes ?? 0) + (r2.OvertimeMinutes ?? 0) + (r3.OvertimeMinutes ?? 0), summary.TotalOvertimeMinutes);
    }

    [Fact]
    public async Task EmptyPeriod_ReturnsZeroTotals()
    {
        var (db, user, time) = NewCtx();
        SeedUser(db, 1);

        var handler = new GetUserAttendanceSummaryQueryHandler(db, user, time);
        var result = await handler.Handle(
            new GetUserAttendanceSummaryQuery(1, new DateOnly(2026, 9, 15), new DateOnly(2026, 9, 15)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.RecordsCount);
        Assert.Equal(0, result.Value.DaysPresent);
        Assert.Equal(0, result.Value.TotalWorkedMinutes);
        Assert.Equal(0, result.Value.TotalLatenessMinutes);
        Assert.Equal(0, result.Value.TotalOvertimeMinutes);
    }

    [Fact]
    public void Validator_Rules()
    {
        var validator = new GetUserAttendanceSummaryValidator();

        var badUser = validator.Validate(new GetUserAttendanceSummaryQuery(0, new DateOnly(2026, 9, 15), new DateOnly(2026, 9, 15)));
        Assert.False(badUser.IsValid);
        Assert.Contains(badUser.Errors, e => e.ErrorMessage == "المستخدم غير موجود.");

        var inverted = validator.Validate(new GetUserAttendanceSummaryQuery(1, new DateOnly(2026, 9, 16), new DateOnly(2026, 9, 15)));
        Assert.False(inverted.IsValid);
        Assert.Contains(inverted.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");

        Assert.True(validator.Validate(new GetUserAttendanceSummaryQuery(1, new DateOnly(2026, 9, 15), new DateOnly(2026, 9, 15))).IsValid);
        Assert.True(validator.Validate(new GetUserAttendanceSummaryQuery(1, null, null)).IsValid);
    }
}
