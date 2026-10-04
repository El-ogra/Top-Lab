using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Queries.GetBandedResultMonitor;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.Statistics;

public class GetBandedResultMonitorQueryHandlerTests
{
    private static readonly DateTime Mar5Noon = new(2026, 3, 5, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Mar5Late = new(2026, 3, 5, 23, 0, 0, DateTimeKind.Utc);

    private static FakeApplicationDbContext NewDb() => new();

    private static GetBandedResultMonitorQueryHandler Handler(FakeApplicationDbContext db, DateTime? utcNow = null)
    {
        var clock = new FakeDateTimeProvider();
        if (utcNow.HasValue)
        {
            clock.UtcNow = utcNow.Value;
        }

        return new GetBandedResultMonitorQueryHandler(db, clock);
    }

    private static GetBandedResultMonitorQuery Query(decimal min, decimal max) => new(
        10,
        new DateOnly(2026, 3, 1),
        new DateOnly(2026, 3, 31),
        min,
        max);

    private static Test MakeTest(int id, string name)
    {
        return Test.Create(
            TestId.Create(id),
            name,
            name,
            name,
            $"T-{id}",
            60,
            100m);
    }

    private static Patient MakePatient(int id, int? referralEntityId = null)
    {
        return Patient.Create(
            PatientId.Create(id),
            $"Patient {id}",
            Sex.Male,
            30,
            AgeUnit.Year,
            new DateTime(2026, 3, 1, 8, 0, 0, DateTimeKind.Utc),
            AccountType.Individual,
            false,
            null,
            null,
            null,
            null,
            null,
            referralEntityId.HasValue ? ExternalEntityId.Create(referralEntityId.Value) : null);
    }

    private static PatientTest Entered(int id, int patientId, int testId, string? value, DateTime? enteredAtUtc)
    {
        var pt = PatientTest.Create(PatientTestId.Create(id), PatientId.Create(patientId), TestId.Create(testId), 100m);

        if (enteredAtUtc.HasValue)
        {
            pt.EnterResult(value, null, 1, enteredAtUtc.Value);
        }

        return pt;
    }

    private static ExternalEntity MakeReferral(int id, string name)
    {
        return ExternalEntity.Create(
            ExternalEntityId.Create(id),
            EntityType.TreatingDoctor,
            name);
    }

    [Fact]
    public async Task Band_MinMaxInclusive_ReturnsOnlyMatchingRows()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "سكر صائم"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(Entered(1, 1, 10, "3.0", Mar5Noon));
        db.PatientTests.Add(Entered(2, 1, 10, "5.0", Mar5Noon));
        db.PatientTests.Add(Entered(3, 1, 10, "7.0", Mar5Noon));

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.TotalCount);
        Assert.Equal(3, result.Value.Rows.Count);
        Assert.Equal("سكر صائم", result.Value.TestName);
    }

    [Fact]
    public async Task Band_ExcludesValuesOutsideRange()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "سكر صائم"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(Entered(1, 1, 10, "2.9", Mar5Noon));
        db.PatientTests.Add(Entered(2, 1, 10, "3.0", Mar5Noon));
        db.PatientTests.Add(Entered(3, 1, 10, "7.0", Mar5Noon));
        db.PatientTests.Add(Entered(4, 1, 10, "7.1", Mar5Noon));

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([3.0m, 7.0m], result.Value!.Rows.Select(r => decimal.Parse(r.ResultValue, System.Globalization.CultureInfo.InvariantCulture)));
    }

    [Fact]
    public async Task Band_NonNumericResultValue_ExcludedNotErrored()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "تحليل"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(Entered(1, 1, 10, "Negative", Mar5Noon));
        db.PatientTests.Add(Entered(2, 1, 10, null, Mar5Noon));
        db.PatientTests.Add(Entered(3, 1, 10, "   ", Mar5Noon));
        db.PatientTests.Add(Entered(4, 1, 10, "5", Mar5Noon));

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalCount);
        Assert.Equal(4, result.Value.Rows.Single().PatientTestId);
    }

    [Fact]
    public async Task Band_ResultOutsidePeriod_Excluded()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "تحليل"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(Entered(1, 1, 10, "5", new DateTime(2026, 2, 28, 23, 59, 59, DateTimeKind.Utc)));
        db.PatientTests.Add(Entered(2, 1, 10, "5", Mar5Noon));
        db.PatientTests.Add(Entered(3, 1, 10, "5", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)));

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalCount);
        Assert.Equal(Mar5Noon, result.Value.Rows.Single().EnteredAtUtc);
    }

    [Fact]
    public async Task PeriodBounds_AreInclusiveCalendarDays_HalfOpenUtc()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "تحليل"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(Entered(1, 1, 10, "5", Mar5Late));
        db.PatientTests.Add(Entered(2, 1, 10, "5", new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc)));
        db.PatientTests.Add(Entered(3, 1, 10, "5", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)));

        var query = new GetBandedResultMonitorQuery(10, new DateOnly(2026, 3, 5), new DateOnly(2026, 3, 31), 3m, 7m);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
    }

    [Fact]
    public async Task Band_UnenteredRow_Excluded()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "تحليل"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(Entered(1, 1, 10, "5", null));
        db.PatientTests.Add(Entered(2, 1, 10, "5", Mar5Noon));

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.Rows.Single().PatientTestId);
    }

    [Fact]
    public async Task Band_ProfileRow_EnteredButNullValue_Excluded()
    {
        // SaveProfileResultsCommandHandler calls EnterResult(null, …): the row has an
        // EnteredAtUtc but no scalar ResultValue (H-3).
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "بروتوكول"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(Entered(1, 1, 10, null, Mar5Noon));
        db.PatientTests.Add(Entered(2, 1, 10, "5", Mar5Noon));

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.Rows.Single().PatientTestId);
    }

    [Fact]
    public async Task Band_DeletedPatient_Excluded()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "تحليل"));
        var live = MakePatient(1);
        var deleted = MakePatient(2);
        deleted.SoftDelete();
        db.Patients.Add(live);
        db.Patients.Add(deleted);
        db.PatientTests.Add(Entered(1, 1, 10, "5", Mar5Noon));
        db.PatientTests.Add(Entered(2, 2, 10, "5", Mar5Noon));

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalCount);
        Assert.Equal(1, result.Value.Rows.Single().PatientId);
    }

    [Fact]
    public async Task Band_ReferralNull_RendersNoReferralLabel()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "تحليل"));
        db.Patients.Add(MakePatient(1, referralEntityId: null));
        db.PatientTests.Add(Entered(1, 1, 10, "5", Mar5Noon));

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("بدون جهة إحالة", result.Value!.Rows.Single().ReferralEntityName);
    }

    [Fact]
    public async Task Band_ReferralResolvedFromExternalEntity()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "تحليل"));
        db.ExternalEntities.Add(MakeReferral(10, "مستشفى النور"));
        db.Patients.Add(MakePatient(1, referralEntityId: 10));
        db.PatientTests.Add(Entered(1, 1, 10, "5", Mar5Noon));

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("مستشفى النور", result.Value!.Rows.Single().ReferralEntityName);
    }

    [Fact]
    public async Task Band_UnknownTestId_NotFound()
    {
        var db = NewDb();

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("التحليل غير موجود.", result.Error.Message);
    }

    [Fact]
    public async Task Band_OtherTestRows_Excluded()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "تحليل"));
        db.Tests.Add(MakeTest(11, "تحليل آخر"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(Entered(1, 1, 10, "5", Mar5Noon));
        db.PatientTests.Add(Entered(2, 1, 11, "5", Mar5Noon));

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalCount);
        Assert.Equal(1, result.Value.Rows.Single().PatientTestId);
    }

    [Fact]
    public async Task Status_Text_MapsLifecycle()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "تحليل"));
        db.Patients.Add(MakePatient(1));

        var enteredOnly = Entered(1, 1, 10, "5", Mar5Noon);
        var reviewed = Entered(2, 1, 10, "5", Mar5Noon);
        reviewed.MarkReviewed(1, Mar5Noon);
        var printed = Entered(3, 1, 10, "5", Mar5Noon);
        printed.MarkReviewed(1, Mar5Noon);
        printed.MarkPrinted(1, Mar5Noon);
        var delivered = Entered(4, 1, 10, "5", Mar5Noon);
        delivered.MarkReviewed(1, Mar5Noon);
        delivered.MarkPrinted(1, Mar5Noon);
        delivered.MarkDelivered(1, Mar5Noon);

        db.PatientTests.Add(enteredOnly);
        db.PatientTests.Add(reviewed);
        db.PatientTests.Add(printed);
        db.PatientTests.Add(delivered);

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var byId = result.Value!.Rows.ToDictionary(r => r.PatientTestId);
        Assert.Equal("غير معتمد", byId[1].StatusText);
        Assert.Equal("معتمد", byId[2].StatusText);
        Assert.Equal("تمت الطباعة", byId[3].StatusText);
        Assert.Equal("تم التسليم", byId[4].StatusText);
    }

    [Fact]
    public async Task Order_Deterministic()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "تحليل"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(Entered(9, 1, 10, "5", Mar5Noon));
        db.PatientTests.Add(Entered(2, 1, 10, "5", Mar5Noon));
        db.PatientTests.Add(Entered(5, 1, 10, "5", Mar5Noon));

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([2, 5, 9], result.Value!.Rows.Select(r => r.PatientTestId));
    }

    [Fact]
    public async Task Order_ByEnteredAtUtc_ThenPatientTestId()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "تحليل"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(Entered(1, 1, 10, "5", Mar5Late));
        db.PatientTests.Add(Entered(2, 1, 10, "5", Mar5Noon));

        var result = await Handler(db).Handle(Query(3m, 7m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([2, 1], result.Value!.Rows.Select(r => r.PatientTestId));
    }

    [Fact]
    public async Task EmptyBand_ReturnsSuccessWithNoRows()
    {
        var db = NewDb();
        db.Tests.Add(MakeTest(10, "تحليل"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(Entered(1, 1, 10, "5", Mar5Noon));

        var result = await Handler(db).Handle(Query(50m, 60m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalCount);
        Assert.Empty(result.Value.Rows);
    }

    [Fact]
    public async Task Defaults_ToToday_WhenDatesOmitted()
    {
        var db = NewDb();
        var today = new DateTime(2026, 6, 15, 14, 30, 0, DateTimeKind.Utc);
        db.Tests.Add(MakeTest(10, "تحليل"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(Entered(1, 1, 10, "5", new DateTime(2026, 6, 15, 1, 0, 0, DateTimeKind.Utc)));
        db.PatientTests.Add(Entered(2, 1, 10, "5", new DateTime(2026, 6, 14, 23, 0, 0, DateTimeKind.Utc)));

        var query = new GetBandedResultMonitorQuery(10, null, null, 3m, 7m);

        var result = await Handler(db, today).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2026, 6, 15), result.Value!.From);
        Assert.Equal(new DateOnly(2026, 6, 15), result.Value.To);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public void From_AfterTo_ValidationFailure()
    {
        var validator = new GetBandedResultMonitorQueryValidator();
        var query = new GetBandedResultMonitorQuery(
            10,
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 1),
            3m,
            7m);

        var outcome = validator.Validate(query);

        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");
    }

    [Fact]
    public void Min_GreaterThanMax_ValidationFailure()
    {
        var validator = new GetBandedResultMonitorQueryValidator();
        var query = new GetBandedResultMonitorQuery(
            10,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            7m,
            3m);

        var outcome = validator.Validate(query);

        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "الحد الأدنى يجب ألا يتجاوز الحد الأقصى.");
    }

    [Fact]
    public void Validator_AllowsEqualBoundsAndOpenPeriods()
    {
        var validator = new GetBandedResultMonitorQueryValidator();

        var equalBand = validator.Validate(new GetBandedResultMonitorQuery(
            10, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 3m, 3m));
        var open = validator.Validate(new GetBandedResultMonitorQuery(
            10, null, new DateOnly(2026, 3, 31), 3m, 7m));
        var bothNull = validator.Validate(new GetBandedResultMonitorQuery(
            10, null, null, 3m, 7m));

        Assert.True(equalBand.IsValid);
        Assert.True(open.IsValid);
        Assert.True(bothNull.IsValid);
    }
}