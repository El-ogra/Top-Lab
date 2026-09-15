using TopLab.Application.Features.Statistics.Queries.GetTestCountStatistics;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.Statistics;

public class GetTestCountStatisticsQueryHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day15 = new(2026, 3, 15, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day20 = new(2026, 3, 20, 12, 0, 0, DateTimeKind.Utc);

    private static FakeApplicationDbContext NewDb() => new();

    private static GetTestCountStatisticsQueryHandler Handler(FakeApplicationDbContext db, DateTime? utcNow = null)
    {
        var clock = new FakeDateTimeProvider();
        if (utcNow.HasValue)
        {
            clock.UtcNow = utcNow.Value;
        }

        return new GetTestCountStatisticsQueryHandler(db, clock);
    }

    private static Patient MakePatient(int id, bool deleted = false)
    {
        var patient = Patient.Create(
            PatientId.Create(id),
            $"Patient {id}",
            Sex.Male,
            30,
            AgeUnit.Year,
            Day1);
        if (deleted)
        {
            patient.SoftDelete();
        }

        return patient;
    }

    private static Test MakeTest(int id, string name, int? groupId)
    {
        return Test.Create(
            TestId.Create(id),
            name,
            name,
            name,
            $"T{id}",
            30,
            10m,
            testGroupId: groupId.HasValue ? TestGroupId.Create(groupId.Value) : null);
    }

    private static PatientTest MakeOrder(int id, int patientId, int testId, DateTime orderedAtUtc)
    {
        var pt = PatientTest.Create(
            PatientTestId.Create(id),
            PatientId.Create(patientId),
            TestId.Create(testId),
            10m);
        pt.CreatedAtUtc = orderedAtUtc;
        return pt;
    }

    [Fact]
    public async Task PerTestAndPerGroup_Counts_NumberForNumber()
    {
        var db = NewDb();
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(1), "دم"));
        db.Tests.Add(MakeTest(10, "CBC", 1));
        db.Tests.Add(MakeTest(11, "HbA1c", 1));
        db.Tests.Add(MakeTest(12, "Vitamin D", null));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(MakeOrder(1, 1, 10, Day1));
        db.PatientTests.Add(MakeOrder(2, 1, 10, Day15));
        db.PatientTests.Add(MakeOrder(3, 1, 11, Day15));
        db.PatientTests.Add(MakeOrder(4, 1, 12, Day20));

        var query = new GetTestCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            TestGroupId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.TotalOrders);
        Assert.Equal(2, result.Value.Tests.Single(t => t.TestId == 10).Count);
        Assert.Equal("CBC", result.Value.Tests.Single(t => t.TestId == 10).TestName);
        Assert.Equal(1, result.Value.Tests.Single(t => t.TestId == 11).Count);
        Assert.Equal(1, result.Value.Tests.Single(t => t.TestId == 12).Count);
        Assert.Equal(3, result.Value.Groups.Single(g => g.TestGroupId == 1).Count);
        Assert.Equal("دم", result.Value.Groups.Single(g => g.TestGroupId == 1).GroupName);
    }

    [Fact]
    public async Task GroupFilter_OnlyCountsThatGroup()
    {
        var db = NewDb();
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(1), "دم"));
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(2), "كيمياء"));
        db.Tests.Add(MakeTest(10, "CBC", 1));
        db.Tests.Add(MakeTest(11, "Glucose", 2));
        db.Tests.Add(MakeTest(12, "Vitamin D", null));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(MakeOrder(1, 1, 10, Day1));
        db.PatientTests.Add(MakeOrder(2, 1, 11, Day1));
        db.PatientTests.Add(MakeOrder(3, 1, 12, Day1));

        var query = new GetTestCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            TestGroupId: 1);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalOrders);
        Assert.Single(result.Value.Tests);
        Assert.Equal(10, result.Value.Tests.Single().TestId);
        Assert.Single(result.Value.Groups);
        Assert.Equal(1, result.Value.Groups.Single().TestGroupId);
    }

    [Fact]
    public async Task UnknownGroupFilter_ReturnsNotFound()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1));

        var query = new GetTestCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            TestGroupId: 999);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("مجموعة التحاليل غير موجودة.", result.Error!.Message);
    }

    [Fact]
    public async Task SoftDeletedPatient_Orders_AreExcluded()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "CBC", 1));
        db.Patients.Add(MakePatient(1));
        db.Patients.Add(MakePatient(2, deleted: true));
        db.PatientTests.Add(MakeOrder(1, 1, 10, Day1));
        db.PatientTests.Add(MakeOrder(2, 2, 10, Day1));

        var query = new GetTestCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            TestGroupId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalOrders);
        Assert.Equal(1, result.Value.Tests.Single().Count);
    }

    [Fact]
    public async Task EmptyPeriod_ReturnsZeros()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "CBC", 1));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(MakeOrder(1, 1, 10, Day1));

        var query = new GetTestCountStatisticsQuery(
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 31),
            TestGroupId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalOrders);
        Assert.Empty(result.Value.Tests);
        Assert.Empty(result.Value.Groups);
    }

    [Fact]
    public void Validator_RejectsInvertedPeriod_WithFrozenMessage()
    {
        var validator = new GetTestCountStatisticsQueryValidator();
        var query = new GetTestCountStatisticsQuery(
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 1),
            TestGroupId: null);

        var outcome = validator.Validate(query);

        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");
    }

    [Fact]
    public async Task DefaultPeriod_IsTodayUtc_WhenOmitted()
    {
        var db = NewDb();
        var today = new DateTime(2026, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        db.Tests.Add(MakeTest(10, "CBC", 1));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(MakeOrder(1, 1, 10, new DateTime(2026, 6, 15, 1, 0, 0, DateTimeKind.Utc)));
        db.PatientTests.Add(MakeOrder(2, 1, 10, new DateTime(2026, 6, 14, 23, 0, 0, DateTimeKind.Utc)));

        var query = new GetTestCountStatisticsQuery(null, null, TestGroupId: null);

        var result = await Handler(db, today).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2026, 6, 15), result.Value!.From);
        Assert.Equal(1, result.Value.TotalOrders);
    }

    [Fact]
    public async Task MissingTestName_FallsBackToRawIdString()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(MakeOrder(1, 1, 77, Day1));

        var query = new GetTestCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            TestGroupId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = result.Value!.Tests.Single();
        Assert.Equal(77, row.TestId);
        Assert.Equal("77", row.TestName);
    }
}
