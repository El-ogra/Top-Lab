using TopLab.Application.Features.Attendance.Commands.EndBreak;
using TopLab.Application.Features.Attendance.Commands.StartBreak;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Attendance;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Application.Tests.Features.Attendance;

public sealed class BreakCommandHandlerTests
{
    private static (FakeApplicationDbContext db, FakeCurrentUserService user, FakeDateTimeProvider time) NewCtx()
        => (new FakeApplicationDbContext(), new FakeCurrentUserService(), new FakeDateTimeProvider());

    private static AttendanceRecord SeedOpen(FakeApplicationDbContext db, int userId = 1)
    {
        var record = AttendanceRecord.Create(
            AttendanceRecordId.Create(0),
            UserId.Create(userId),
            new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc));
        db.AttendanceRecords.Add(record);
        return record;
    }

    [Fact]
    public async Task StartBreak_HappyPath_SetsBreakStart()
    {
        var (db, user, time) = NewCtx();
        time.UtcNow = new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc);
        SeedOpen(db);

        var handler = new StartBreakCommandHandler(db, user, time);
        var result = await handler.Handle(new StartBreakCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(time.UtcNow, Assert.Single(db.AttendanceRecords).BreakStartAtUtc);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task StartBreak_NoOpenRecord_NotFound()
    {
        var (db, user, time) = NewCtx();

        var handler = new StartBreakCommandHandler(db, user, time);
        var result = await handler.Handle(new StartBreakCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يوجد تسجيل حضور مفتوح.", result.Error!.Message);
    }

    [Fact]
    public async Task StartBreak_DoubleStart_TranslatedConflict()
    {
        var (db, user, time) = NewCtx();
        SeedOpen(db);

        var handler = new StartBreakCommandHandler(db, user, time);
        Assert.True((await handler.Handle(new StartBreakCommand(), CancellationToken.None)).IsSuccess);

        var retry = await handler.Handle(new StartBreakCommand(), CancellationToken.None);

        Assert.False(retry.IsSuccess);
        Assert.Equal("الاستراحة بدأت بالفعل.", retry.Error!.Message);
    }

    [Fact]
    public async Task StartBreak_Unauthenticated_Forbidden()
    {
        var (db, user, time) = NewCtx();
        user.ClearSession();
        SeedOpen(db);

        var handler = new StartBreakCommandHandler(db, user, time);
        var result = await handler.Handle(new StartBreakCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", result.Error!.Message);
    }

    [Fact]
    public async Task EndBreak_HappyPath_SetsBreakEnd()
    {
        var (db, user, time) = NewCtx();
        var record = SeedOpen(db);
        record.StartBreak(new DateTime(2026, 9, 15, 10, 0, 0, DateTimeKind.Utc));
        time.UtcNow = new DateTime(2026, 9, 15, 10, 30, 0, DateTimeKind.Utc);

        var handler = new EndBreakCommandHandler(db, user, time);
        var result = await handler.Handle(new EndBreakCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(time.UtcNow, Assert.Single(db.AttendanceRecords).BreakEndAtUtc);
    }

    [Fact]
    public async Task EndBreak_WithoutStart_TranslatedConflict()
    {
        var (db, user, time) = NewCtx();
        SeedOpen(db);

        var handler = new EndBreakCommandHandler(db, user, time);
        var result = await handler.Handle(new EndBreakCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا توجد استراحة مفتوحة.", result.Error!.Message);
    }

    [Fact]
    public async Task EndBreak_NoOpenRecord_NotFound()
    {
        var (db, user, time) = NewCtx();

        var handler = new EndBreakCommandHandler(db, user, time);
        var result = await handler.Handle(new EndBreakCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يوجد تسجيل حضور مفتوح.", result.Error!.Message);
    }

    [Fact]
    public async Task EndBreak_Unauthenticated_Forbidden()
    {
        var (db, user, time) = NewCtx();
        user.ClearSession();
        SeedOpen(db);

        var handler = new EndBreakCommandHandler(db, user, time);
        var result = await handler.Handle(new EndBreakCommand(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", result.Error!.Message);
    }

    [Fact]
    public void Validators_ParameterlessCommands_AreValid()
    {
        Assert.True(new StartBreakCommandValidator().Validate(new StartBreakCommand()).IsValid);
        Assert.True(new EndBreakCommandValidator().Validate(new EndBreakCommand()).IsValid);
    }
}
