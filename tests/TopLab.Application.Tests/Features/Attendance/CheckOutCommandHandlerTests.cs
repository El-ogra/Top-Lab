using TopLab.Application.Features.Attendance.Commands.CheckOut;
using TopLab.Application.Features.Attendance.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Attendance;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.Attendance;

public sealed class CheckOutCommandHandlerTests
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

    private static AttendanceRecord SeedOpen(FakeApplicationDbContext db, int userId = 1)
    {
        var record = AttendanceRecord.Create(
            AttendanceRecordId.Create(0),
            UserId.Create(userId),
            new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc));
        db.AttendanceRecords.Add(record);
        return record;
    }

    private static TimeOnly LocalOf(DateTime utc) => TimeOnly.FromDateTime(utc.ToLocalTime());

    [Fact]
    public async Task HappyPath_WithComputedOvertime()
    {
        var (db, user, time) = NewCtx();
        time.UtcNow = new DateTime(2026, 9, 15, 14, 30, 0, DateTimeKind.Utc);
        SeedUser(db, 1, LocalOf(time.UtcNow).Add(TimeSpan.FromHours(-8)), LocalOf(time.UtcNow).Add(TimeSpan.FromMinutes(-30)));
        SeedOpen(db);

        var handler = new CheckOutCommandHandler(db, user, time);
        var result = await handler.Handle(new CheckOutCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(db.AttendanceRecords);
        Assert.Equal(time.UtcNow, stored.CheckOutAtUtc);
        Assert.Equal(30, stored.OvertimeMinutes);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task NoConfiguredHours_OvertimeNull_StillSucceeds()
    {
        var (db, user, time) = NewCtx();
        time.UtcNow = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);
        SeedUser(db, 1, null, null);
        SeedOpen(db);

        var handler = new CheckOutCommandHandler(db, user, time);
        var result = await handler.Handle(new CheckOutCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(db.AttendanceRecords).OvertimeMinutes);
    }

    [Fact]
    public async Task MissingUser_OvertimeNull_StillSucceeds()
    {
        var (db, user, time) = NewCtx();
        time.UtcNow = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);
        SeedOpen(db);

        var handler = new CheckOutCommandHandler(db, user, time);
        var result = await handler.Handle(new CheckOutCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(db.AttendanceRecords).OvertimeMinutes);
    }

    [Fact]
    public async Task OnTimeCheckOut_OvertimeZero()
    {
        var (db, user, time) = NewCtx();
        time.UtcNow = new DateTime(2026, 9, 15, 14, 0, 0, DateTimeKind.Utc);
        SeedUser(db, 1, LocalOf(time.UtcNow).Add(TimeSpan.FromHours(-8)), LocalOf(time.UtcNow));
        SeedOpen(db);

        var handler = new CheckOutCommandHandler(db, user, time);
        var result = await handler.Handle(new CheckOutCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, Assert.Single(db.AttendanceRecords).OvertimeMinutes);
    }

    [Fact]
    public async Task OpenBreak_RejectedWithTranslatedMessage()
    {
        var (db, user, time) = NewCtx();
        SeedUser(db, 1, null, null);
        var record = SeedOpen(db);
        record.StartBreak(new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc));

        var handler = new CheckOutCommandHandler(db, user, time);
        var result = await handler.Handle(new CheckOutCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("أنهِ الاستراحة قبل تسجيل الانصراف.", result.Error!.Message);
        Assert.Null(Assert.Single(db.AttendanceRecords).CheckOutAtUtc);
    }

    [Fact]
    public async Task NoOpenRecord_NotFound()
    {
        var (db, user, time) = NewCtx();
        SeedUser(db, 1, null, null);

        var handler = new CheckOutCommandHandler(db, user, time);
        var result = await handler.Handle(new CheckOutCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يوجد تسجيل حضور مفتوح.", result.Error!.Message);
    }

    [Fact]
    public async Task Unauthenticated_Forbidden()
    {
        var (db, user, time) = NewCtx();
        user.ClearSession();
        SeedOpen(db);

        var handler = new CheckOutCommandHandler(db, user, time);
        var result = await handler.Handle(new CheckOutCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", result.Error!.Message);
    }

    [Fact]
    public void Validator_ParameterlessCommand_IsValid()
    {
        Assert.True(new CheckOutCommandValidator().Validate(new CheckOutCommand()).IsValid);
    }

    [Fact]
    public void DomainFailureTranslator_MapsAllArms()
    {
        Assert.Equal("الاستراحة بدأت بالفعل.", DomainFailureTranslator.Translate(new InvalidOperationException("Break is already started.")));
        Assert.Equal("لا توجد استراحة مفتوحة.", DomainFailureTranslator.Translate(new InvalidOperationException("No open break to end.")));
        Assert.Equal("أنهِ الاستراحة قبل تسجيل الانصراف.", DomainFailureTranslator.Translate(new InvalidOperationException("Cannot check out with an open break.")));
        Assert.Equal("تم تسجيل الانصراف مسبقًا.", DomainFailureTranslator.Translate(new InvalidOperationException("Already checked out.")));
        Assert.Equal("بيانات غير صالحة.", DomainFailureTranslator.Translate(new InvalidOperationException("boom")));
    }
}
