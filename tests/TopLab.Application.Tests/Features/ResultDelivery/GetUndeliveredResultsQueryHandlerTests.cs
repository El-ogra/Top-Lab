using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.PatientStatus;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultDelivery;

public class GetUndeliveredResultsQueryHandlerTests
{
    private static readonly DateTime Day = new(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);

    private static Patient MakePatient(int id, DateTime? registration = null, string name = "Ahmed")
    {
        return Patient.Create(PatientId.Create(id), $"{name}{id}", Sex.Male, 30, AgeUnit.Year, registration ?? Day);
    }

    private static void AddTest(FakeApplicationDbContext db, int id)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}-R", $"T{id}", 30, 100m));
    }

    private static PatientTest Row(int ptId, int patientId, int testId, bool delivered, bool printed = true)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, Day);
        pt.MarkReviewed(1, Day);
        if (printed)
            pt.MarkPrinted(1, Day);
        if (delivered)
            pt.MarkDelivered(1, Day);
        return pt;
    }

    private static DateOnly D(DateTime dt) => DateOnly.FromDateTime(dt);

    [Fact]
    public async Task PeriodFilter_InclusiveUtcDays_OnlyUndeliveredPatientsListed()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, Day));
        db.Patients.Add(MakePatient(2, Day.AddDays(1)));
        db.Patients.Add(MakePatient(3, Day.AddDays(5)));
        AddTest(db, 10);
        db.PatientTests.Add(Row(100, 1, 10, delivered: false));
        db.PatientTests.Add(Row(101, 2, 10, delivered: false));
        db.PatientTests.Add(Row(102, 3, 10, delivered: false));

        var handler = new GetUndeliveredResultsQueryHandler(db);
        var result = await handler.Handle(
            new GetUndeliveredResultsQuery(D(Day), D(Day.AddDays(1))), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value, r => r.PatientId == 1 && r.UndeliveredCount == 1);
        Assert.Contains(result.Value, r => r.PatientId == 2 && r.UndeliveredCount == 1);
    }

    [Fact]
    public async Task FullyDeliveredPatient_AbsentFromList()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddTest(db, 10);
        db.PatientTests.Add(Row(100, 1, 10, delivered: true));

        var handler = new GetUndeliveredResultsQueryHandler(db);
        var result = await handler.Handle(
            new GetUndeliveredResultsQuery(D(Day), D(Day)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task SoftDeletedPatient_Excluded()
    {
        var db = new FakeApplicationDbContext();
        var patient = MakePatient(1);
        patient.SoftDelete();
        db.Patients.Add(patient);
        AddTest(db, 10);
        db.PatientTests.Add(Row(100, 1, 10, delivered: false));

        var handler = new GetUndeliveredResultsQueryHandler(db);
        var result = await handler.Handle(
            new GetUndeliveredResultsQuery(D(Day), D(Day)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task UndeliveredCount_CountsOnlyUndeliveredLines()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddTest(db, 10);
        AddTest(db, 11);
        db.PatientTests.Add(Row(100, 1, 10, delivered: false));
        db.PatientTests.Add(Row(101, 1, 11, delivered: false));
        db.PatientTests.Add(Row(102, 1, 10, delivered: true));

        var handler = new GetUndeliveredResultsQueryHandler(db);
        var result = await handler.Handle(
            new GetUndeliveredResultsQuery(D(Day), D(Day)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(result.Value!);
        Assert.Equal(2, row.UndeliveredCount);
        Assert.Equal("Ahmed1", row.PatientFullName);
        Assert.Equal(Day, row.RegistrationDateUtc);
    }

    [Fact]
    public async Task Status_Matches_PatientStatusCalculator_Verbatim()
    {
        var db = new FakeApplicationDbContext();
        var patient = MakePatient(1);
        db.Patients.Add(patient);
        AddTest(db, 10);
        db.PatientTests.Add(Row(100, 1, 10, delivered: false));

        var handler = new GetUndeliveredResultsQueryHandler(db);
        var result = await handler.Handle(
            new GetUndeliveredResultsQuery(D(Day), D(Day)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = Assert.Single(result.Value!);
        var allTests = db.PatientTests.Where(pt => pt.PatientId.Value == 1).ToList();
        var expected = new PatientStatusCalculator().Calculate(
            patient,
            allTests,
            PatientAccountCalculator.Balance(
                allTests.Select(t => t.PriceAtOrderTime).ToList(),
                db.PaymentOperations.ToList()));
        Assert.Equal((int)expected, row.VisitStatus);
    }

    [Fact]
    public async Task DefaultPeriod_TodayUtc_WhenOmitted()
    {
        var db = new FakeApplicationDbContext();
        var todayPatient = Patient.Create(PatientId.Create(1), "Today1", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        var oldPatient = Patient.Create(PatientId.Create(2), "Old2", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow.AddDays(-9));
        db.Patients.Add(todayPatient);
        db.Patients.Add(oldPatient);
        AddTest(db, 10);
        db.PatientTests.Add(Row(100, 1, 10, delivered: false));
        db.PatientTests.Add(Row(101, 2, 10, delivered: false));

        var handler = new GetUndeliveredResultsQueryHandler(db);
        var result = await handler.Handle(new GetUndeliveredResultsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value!, r => r.PatientId == 1);
        Assert.DoesNotContain(result.Value!, r => r.PatientId == 2);
    }

    [Fact]
    public void Validator_InvertedPeriod_RejectsWithFrozenMessage()
    {
        var validator = new GetUndeliveredResultsQueryValidator();
        var result = validator.Validate(new GetUndeliveredResultsQuery(D(Day.AddDays(2)), D(Day)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");
    }

    [Theory]
    [InlineData(0, 50)]
    [InlineData(1, 0)]
    [InlineData(1, 501)]
    public void Validator_InvalidPaging_Rejects(int page, int pageSize)
    {
        var validator = new GetUndeliveredResultsQueryValidator();
        var result = validator.Validate(new GetUndeliveredResultsQuery(D(Day), D(Day), page, pageSize));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "معاملات الترقيم غير صالحة.");
    }
}
