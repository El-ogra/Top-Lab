using TopLab.Application.Features.Attendance.Commands.CheckIn;
using TopLab.Application.Features.Attendance.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Attendance;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.Attendance;

public sealed class CheckInCommandHandlerTests
{
    private static (FakeApplicationDbContext db, FakeCurrentUserService user, FakeDateTimeProvider time) NewCtx()
        => (new FakeApplicationDbContext(), new FakeCurrentUserService(), new FakeDateTimeProvider());

    private static void SeedUser(FakeApplicationDbContext db, int id, TimeOnly? workStart, TimeOnly? workEnd)
    {
        db.Users.Add(User.Create(
            UserId.Create(id),
            $"user{id}",
            "pwdhash",
            "internalhash",
            workStartTime: workStart,
            workEndTime: workEnd));
    }

    private static TimeOnly LocalOf(DateTime utc) => TimeOnly.FromDateTime(utc.ToLocalTime());

    [Fact]
    public async Task HappyPath_RecordsForCurrentUser_WithComputedLateness()
    {
        var (db, user, time) = NewCtx();
        user.UserId = 42;
        time.UtcNow = new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);
        SeedUser(db, 42, LocalOf(time.UtcNow).Add(TimeSpan.FromMinutes(-30)), LocalOf(time.UtcNow).Add(TimeSpan.FromHours(8)));

        var handler = new CheckInCommandHandler(db, user, time);
        var result = await handler.Handle(new CheckInCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(db.AttendanceRecords);
        Assert.Equal(42, stored.UserId.Value);
        Assert.Equal(time.UtcNow, stored.CheckInAtUtc);
        Assert.Equal(30, stored.LatenessMinutes);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task NoConfiguredHours_LatenessNull_StillSucceeds()
    {
        var (db, user, time) = NewCtx();
        time.UtcNow = new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);
        SeedUser(db, 1, null, null);

        var handler = new CheckInCommandHandler(db, user, time);
        var result = await handler.Handle(new CheckInCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(db.AttendanceRecords).LatenessMinutes);
    }

    [Fact]
    public async Task MissingUser_LatenessNull_StillSucceeds()
    {
        var (db, user, time) = NewCtx();
        time.UtcNow = new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);

        var handler = new CheckInCommandHandler(db, user, time);
        var result = await handler.Handle(new CheckInCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(db.AttendanceRecords).LatenessMinutes);
    }

    [Fact]
    public async Task OnTimeCheckIn_LatenessZero()
    {
        var (db, user, time) = NewCtx();
        time.UtcNow = new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);
        SeedUser(db, 1, LocalOf(time.UtcNow), LocalOf(time.UtcNow).Add(TimeSpan.FromHours(8)));

        var handler = new CheckInCommandHandler(db, user, time);
        var result = await handler.Handle(new CheckInCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, Assert.Single(db.AttendanceRecords).LatenessMinutes);
    }

    [Fact]
    public async Task OpenRecordExists_RejectedWithConflict()
    {
        var (db, user, time) = NewCtx();
        SeedUser(db, 1, null, null);

        var first = new CheckInCommandHandler(db, user, time);
        Assert.True((await first.Handle(new CheckInCommand(), CancellationToken.None)).IsSuccess);

        var second = new CheckInCommandHandler(db, user, time);
        var result = await second.Handle(new CheckInCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("يوجد تسجيل حضور مفتوح لهذا المستخدم.", result.Error!.Message);
        Assert.Single(db.AttendanceRecords);
    }

    [Fact]
    public async Task Unauthenticated_Forbidden_NoSave()
    {
        var (db, user, time) = NewCtx();
        user.ClearSession();

        var handler = new CheckInCommandHandler(db, user, time);
        var result = await handler.Handle(new CheckInCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", result.Error!.Message);
        Assert.Empty(db.AttendanceRecords);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public void Validator_ParameterlessCommand_IsValid()
    {
        var validator = new CheckInCommandValidator();

        Assert.True(validator.Validate(new CheckInCommand()).IsValid);
    }

    [Fact]
    public void AttendanceRecordDto_OpenRecord_WorkedMinutesNull()
    {
        var record = AttendanceRecord.Create(
            AttendanceRecordId.Create(0),
            UserId.Create(5),
            new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc),
            latenessMinutes: 3);

        var dto = AttendanceRecordDto.FromRecord(record, "user5");

        Assert.Equal(0, dto.Id);
        Assert.Equal(5, dto.UserId);
        Assert.Equal("user5", dto.UserName);
        Assert.Equal(3, dto.LatenessMinutes);
        Assert.Null(dto.CheckOutAtUtc);
        Assert.Null(dto.WorkedMinutes);
    }

    [Fact]
    public void AttendanceRecordDto_ClosedWithBreak_WorkedMinutesExcludeBreak()
    {
        var record = AttendanceRecord.Create(
            AttendanceRecordId.Create(0),
            UserId.Create(5),
            new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc));
        record.StartBreak(new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc));
        record.EndBreak(new DateTime(2026, 9, 15, 10, 30, 0, DateTimeKind.Utc));
        record.CheckOut(new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc), overtimeMinutes: 0);

        var dto = AttendanceRecordDto.FromRecord(record, "user5");

        Assert.Equal(450, dto.WorkedMinutes);
        Assert.Equal(0, dto.OvertimeMinutes);
    }
}
