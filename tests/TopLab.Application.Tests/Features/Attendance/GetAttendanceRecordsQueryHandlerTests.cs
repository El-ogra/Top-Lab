using TopLab.Application.Features.Attendance.Queries.GetAttendanceRecords;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Attendance;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.Attendance;

public sealed class GetAttendanceRecordsQueryHandlerTests
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

    private static void SeedRecord(
        FakeApplicationDbContext db,
        int userId,
        DateTime checkIn,
        int? lateness = null,
        DateTime? checkOut = null,
        int? overtime = null)
    {
        var record = AttendanceRecord.Create(
            AttendanceRecordId.Create(0),
            UserId.Create(userId),
            checkIn,
            lateness);
        db.AttendanceRecords.Add(record);
        if (checkOut.HasValue)
        {
            record.CheckOut(checkOut.Value, overtime);
        }
    }

    private static GetAttendanceRecordsQuery Query(
        int? userId = null,
        DateOnly? from = null,
        DateOnly? to = null,
        int page = 1,
        int pageSize = 50)
        => new(userId, from, to, page, pageSize);

    [Fact]
    public async Task NonAbsolute_Forbidden_Verbatim()
    {
        var (db, user, time) = NewCtx(absolute: false);
        SeedUser(db, 1);

        var handler = new GetAttendanceRecordsQueryHandler(db, user, time);
        var result = await handler.Handle(Query(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام", result.Error!.Message);
    }

    [Fact]
    public async Task Absolute_ReturnsOrderedRecords_WithResolvedNames()
    {
        var (db, user, time) = NewCtx();
        SeedUser(db, 1);
        SeedUser(db, 2);
        SeedRecord(db, 2, new DateTime(2026, 9, 15, 8, 0, 0), lateness: 1);
        SeedRecord(db, 1, new DateTime(2026, 9, 15, 6, 0, 0), lateness: 0, checkOut: new DateTime(2026, 9, 15, 14, 0, 0), overtime: 0);

        var handler = new GetAttendanceRecordsQueryHandler(db, user, time);
        var result = await handler.Handle(
            Query(from: new DateOnly(2026, 9, 15), to: new DateOnly(2026, 9, 15)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal(1, result.Value[0].UserId);
        Assert.Equal("user1", result.Value[0].UserName);
        Assert.Equal(480, result.Value[0].WorkedMinutes);
        Assert.Equal(2, result.Value[1].UserId);
        Assert.Equal("user2", result.Value[1].UserName);
        Assert.Null(result.Value[1].WorkedMinutes);
    }

    [Fact]
    public async Task Period_InclusiveBothEnds()
    {
        var (db, user, time) = NewCtx();
        SeedUser(db, 1);
        SeedRecord(db, 1, new DateTime(2026, 9, 14, 0, 0, 0));
        SeedRecord(db, 1, new DateTime(2026, 9, 15, 23, 59, 59));
        SeedRecord(db, 1, new DateTime(2026, 9, 13, 23, 59, 59));
        SeedRecord(db, 1, new DateTime(2026, 9, 16, 0, 0, 0));

        var handler = new GetAttendanceRecordsQueryHandler(db, user, time);
        var result = await handler.Handle(
            Query(from: new DateOnly(2026, 9, 14), to: new DateOnly(2026, 9, 15)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task UserFilter_ReturnsOnlyThatUser()
    {
        var (db, user, time) = NewCtx();
        SeedUser(db, 1);
        SeedUser(db, 2);
        SeedRecord(db, 1, new DateTime(2026, 9, 15, 6, 0, 0));
        SeedRecord(db, 2, new DateTime(2026, 9, 15, 7, 0, 0));

        var handler = new GetAttendanceRecordsQueryHandler(db, user, time);
        var result = await handler.Handle(
            Query(userId: 2, from: new DateOnly(2026, 9, 15), to: new DateOnly(2026, 9, 15)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var single = Assert.Single(result.Value!);
        Assert.Equal(2, single.UserId);
    }

    [Fact]
    public async Task EmptySet_ReturnsSuccessEmpty()
    {
        var (db, user, time) = NewCtx();
        SeedUser(db, 1);

        var handler = new GetAttendanceRecordsQueryHandler(db, user, time);
        var result = await handler.Handle(
            Query(from: new DateOnly(2026, 9, 15), to: new DateOnly(2026, 9, 15)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task DefaultToday_ReturnsOnlyTodayUtc()
    {
        var (db, user, time) = NewCtx();
        time.UtcNow = new DateTime(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc);
        SeedUser(db, 1);
        SeedRecord(db, 1, new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc));
        SeedRecord(db, 1, new DateTime(2026, 9, 14, 6, 0, 0, DateTimeKind.Utc));

        var handler = new GetAttendanceRecordsQueryHandler(db, user, time);
        var result = await handler.Handle(Query(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var single = Assert.Single(result.Value!);
        Assert.Equal(new DateTime(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc), single.CheckInAtUtc);
    }

    [Fact]
    public async Task UnknownUser_ResolvesEmptyName()
    {
        var (db, user, time) = NewCtx();
        SeedRecord(db, 77, new DateTime(2026, 9, 15, 6, 0, 0));

        var handler = new GetAttendanceRecordsQueryHandler(db, user, time);
        var result = await handler.Handle(
            Query(from: new DateOnly(2026, 9, 15), to: new DateOnly(2026, 9, 15)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, Assert.Single(result.Value!).UserName);
    }

    [Fact]
    public void Validator_Rules()
    {
        var validator = new GetAttendanceRecordsQueryValidator();

        var inverted = validator.Validate(Query(from: new DateOnly(2026, 9, 16), to: new DateOnly(2026, 9, 15)));
        Assert.False(inverted.IsValid);
        Assert.Contains(inverted.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");

        var badPage = validator.Validate(Query(from: new DateOnly(2026, 9, 15), to: new DateOnly(2026, 9, 15), page: 0));
        Assert.False(badPage.IsValid);
        Assert.Contains(badPage.Errors, e => e.ErrorMessage == "معاملات الترقيم غير صالحة.");

        var badSize = validator.Validate(Query(from: new DateOnly(2026, 9, 15), to: new DateOnly(2026, 9, 15), pageSize: 101));
        Assert.False(badSize.IsValid);
        Assert.Contains(badSize.Errors, e => e.ErrorMessage == "معاملات الترقيم غير صالحة.");

        Assert.True(validator.Validate(Query(from: new DateOnly(2026, 9, 15), to: new DateOnly(2026, 9, 15))).IsValid);
        Assert.True(validator.Validate(Query()).IsValid);
        Assert.True(validator.Validate(Query(from: new DateOnly(2026, 9, 15))).IsValid);
        Assert.True(validator.Validate(Query(to: new DateOnly(2026, 9, 15))).IsValid);
    }
}
