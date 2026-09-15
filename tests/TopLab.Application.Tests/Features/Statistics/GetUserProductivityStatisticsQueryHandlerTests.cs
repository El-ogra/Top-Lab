using TopLab.Application.Features.Statistics.Queries.GetUserProductivityStatistics;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.Statistics;

public class GetUserProductivityStatisticsQueryHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day15 = new(2026, 3, 15, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day20 = new(2026, 3, 20, 12, 0, 0, DateTimeKind.Utc);

    private static FakeApplicationDbContext NewDb() => new();

    private static GetUserProductivityStatisticsQueryHandler Handler(FakeApplicationDbContext db, DateTime? utcNow = null)
    {
        var clock = new FakeDateTimeProvider();
        if (utcNow.HasValue)
        {
            clock.UtcNow = utcNow.Value;
        }

        return new GetUserProductivityStatisticsQueryHandler(db, clock);
    }

    private static User MakeUser(int id, string name)
    {
        return User.Create(UserId.Create(id), name, "hash", "winhash");
    }

    private static PatientTest MakeRow(int id)
    {
        return PatientTest.Create(
            PatientTestId.Create(id),
            PatientId.Create(1),
            TestId.Create(1),
            10m);
    }

    [Fact]
    public async Task EachAttribution_CountedOnItsOwnTimestamp()
    {
        var db = NewDb();
        db.Users.Add(MakeUser(1, "tech-a"));
        db.Users.Add(MakeUser(2, "tech-b"));
        db.Patients.Add(Patient.Create(PatientId.Create(1), "P", Sex.Male, 30, AgeUnit.Year, Day1));

        var row = MakeRow(1);
        row.EnterResult("1", null, 1, Day1);
        row.MarkReviewed(2, Day15);
        row.MarkPrinted(1, Day20);
        row.MarkDelivered(2, Day20);
        db.PatientTests.Add(row);

        var query = new GetUserProductivityStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            UserId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var user1 = result.Value!.Users.Single(u => u.UserId == 1);
        var user2 = result.Value.Users.Single(u => u.UserId == 2);
        Assert.Equal(1, user1.EnteredCount);
        Assert.Equal(0, user1.ReviewedCount);
        Assert.Equal(1, user1.PrintedCount);
        Assert.Equal(0, user1.DeliveredCount);
        Assert.Equal(0, user2.EnteredCount);
        Assert.Equal(1, user2.ReviewedCount);
        Assert.Equal(0, user2.PrintedCount);
        Assert.Equal(1, user2.DeliveredCount);
    }

    [Fact]
    public async Task OnlyPrintsInPeriod_AppearsWithPrintedCountOnly()
    {
        var db = NewDb();
        db.Users.Add(MakeUser(3, "printer"));
        db.Users.Add(MakeUser(1, "entry"));
        var row = MakeRow(1);
        row.EnterResult("1", null, 1, new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));
        row.MarkReviewed(1, new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc));
        row.MarkPrinted(3, Day15);
        db.PatientTests.Add(row);

        var query = new GetUserProductivityStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            UserId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Users);
        var user = result.Value.Users.Single(u => u.UserId == 3);
        Assert.Equal("printer", user.UserName);
        Assert.Equal(0, user.EnteredCount);
        Assert.Equal(0, user.ReviewedCount);
        Assert.Equal(1, user.PrintedCount);
        Assert.Equal(0, user.DeliveredCount);
    }

    [Fact]
    public async Task Reprints_SumIntoLastPrintersPrintedCount()
    {
        var db = NewDb();
        db.Users.Add(MakeUser(1, "a"));
        var row = MakeRow(1);
        row.EnterResult("1", null, 1, Day1);
        row.MarkReviewed(1, Day1);
        row.MarkPrinted(1, Day1);
        row.MarkPrinted(1, Day15);
        db.PatientTests.Add(row);

        var query = new GetUserProductivityStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            UserId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Users.Single().PrintedCount);
    }

    [Fact]
    public async Task ZeroActivityUsers_AreOmitted()
    {
        var db = NewDb();
        db.Users.Add(MakeUser(1, "idle"));
        db.Patients.Add(Patient.Create(PatientId.Create(1), "P", Sex.Male, 30, AgeUnit.Year, Day1));

        var query = new GetUserProductivityStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            UserId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Users);
    }

    [Fact]
    public async Task DeletedUser_FallsBackToRawIdString()
    {
        var db = NewDb();
        var row = MakeRow(1);
        row.EnterResult("1", null, 99, Day1);
        db.PatientTests.Add(row);

        var query = new GetUserProductivityStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            UserId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var user = result.Value!.Users.Single();
        Assert.Equal(99, user.UserId);
        Assert.Equal("99", user.UserName);
    }

    [Fact]
    public async Task ActivityOutsidePeriod_IsExcluded()
    {
        var db = NewDb();
        db.Users.Add(MakeUser(1, "a"));
        var row = MakeRow(1);
        row.EnterResult("1", null, 1, new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));
        db.PatientTests.Add(row);

        var query = new GetUserProductivityStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            UserId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Users);
    }

    [Fact]
    public void Validator_RejectsInvertedPeriod_WithFrozenMessage()
    {
        var validator = new GetUserProductivityStatisticsQueryValidator();
        var query = new GetUserProductivityStatisticsQuery(
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 1),
            UserId: null);

        var outcome = validator.Validate(query);

        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
