using TopLab.Application.Features.ResultsEntry.Queries.GetResultWorklist;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public class GetResultWorklistQueryHandlerTests
{
    private static Patient MakePatient(int id, string name, DateTime? registrationUtc = null)
    {
        return Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, registrationUtc ?? DateTime.UtcNow);
    }

    private static void AddCatalogTest(FakeApplicationDbContext db, int id, string name, int? groupId = null, ResultKind kind = ResultKind.Simple, bool culture = false)
    {
        TestGroupId? gid = groupId.HasValue ? TestGroupId.Create(groupId.Value) : null;
        db.Tests.Add(Test.Create(TestId.Create(id), name, name, name, $"T{id}", 30, 100m, kind, culture, gid));
    }

    private static PatientTest MakeRow(int ptId, int patientId, int testId)
    {
        return PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);
    }

    [Fact]
    public async Task DayFilter_DefaultsToToday()
    {
        var db = new FakeApplicationDbContext();
        var now = DateTime.UtcNow;
        db.Patients.Add(MakePatient(1, "Today", now));
        db.Patients.Add(MakePatient(2, "Yesterday", now.AddDays(-1)));
        AddCatalogTest(db, 10, "CBC");
        db.PatientTests.Add(MakeRow(101, 1, 10));
        db.PatientTests.Add(MakeRow(102, 2, 10));

        var handler = new GetResultWorklistQueryHandler(db);
        var result = await handler.Handle(new GetResultWorklistQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(1, result.Value![0].PatientId);
    }

    [Fact]
    public async Task Excludes_SoftDeleted_Patients()
    {
        var db = new FakeApplicationDbContext();
        var deleted = MakePatient(1, "Deleted");
        deleted.SoftDelete();
        db.Patients.Add(deleted);
        AddCatalogTest(db, 10, "CBC");
        db.PatientTests.Add(MakeRow(101, 1, 10));

        var handler = new GetResultWorklistQueryHandler(db);
        var result = await handler.Handle(new GetResultWorklistQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task HasResult_Filter_Works()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "P1"));
        AddCatalogTest(db, 10, "CBC");
        var entered = MakeRow(101, 1, 10);
        entered.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        var pending = MakeRow(102, 1, 10);
        db.PatientTests.Add(entered);
        db.PatientTests.Add(pending);

        var handler = new GetResultWorklistQueryHandler(db);

        var withResult = await handler.Handle(new GetResultWorklistQuery(HasResult: true), CancellationToken.None);
        Assert.True(withResult.IsSuccess);
        Assert.Single(withResult.Value!);
        Assert.Equal(101, withResult.Value![0].PatientTestId);

        var without = await handler.Handle(new GetResultWorklistQuery(HasResult: false), CancellationToken.None);
        Assert.True(without.IsSuccess);
        Assert.Single(without.Value!);
        Assert.Equal(102, without.Value![0].PatientTestId);
    }

    [Fact]
    public async Task IsReviewed_Filter_Works()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "P1"));
        AddCatalogTest(db, 10, "CBC");
        var reviewed = MakeRow(101, 1, 10);
        reviewed.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        reviewed.MarkReviewed(1, DateTime.UtcNow);
        var unreviewed = MakeRow(102, 1, 10);
        unreviewed.EnterResult("6", ResultFlag.Normal, 1, DateTime.UtcNow);
        db.PatientTests.Add(reviewed);
        db.PatientTests.Add(unreviewed);

        var handler = new GetResultWorklistQueryHandler(db);
        var result = await handler.Handle(new GetResultWorklistQuery(IsReviewed: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(101, result.Value![0].PatientTestId);
    }

    [Fact]
    public async Task TestGroup_Filter_Works()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "P1"));
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(5), "G5"));
        db.TestGroups.Add(TestGroup.Create(TestGroupId.Create(6), "G6"));
        AddCatalogTest(db, 10, "CBC", groupId: 5);
        AddCatalogTest(db, 11, "Glucose", groupId: 6);
        db.PatientTests.Add(MakeRow(101, 1, 10));
        db.PatientTests.Add(MakeRow(102, 1, 11));

        var handler = new GetResultWorklistQueryHandler(db);
        var result = await handler.Handle(new GetResultWorklistQuery(TestGroupId: 5), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(101, result.Value![0].PatientTestId);
    }

    [Fact]
    public async Task ResultKind_Filter_Works()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "P1"));
        AddCatalogTest(db, 10, "Simple", kind: ResultKind.Simple);
        AddCatalogTest(db, 11, "Profile", kind: ResultKind.SpecializedProfile);
        AddCatalogTest(db, 12, "Culture", kind: ResultKind.Culture, culture: true);
        db.PatientTests.Add(MakeRow(101, 1, 10));
        db.PatientTests.Add(MakeRow(102, 1, 11));
        db.PatientTests.Add(MakeRow(103, 1, 12));

        var handler = new GetResultWorklistQueryHandler(db);
        var simple = await handler.Handle(new GetResultWorklistQuery(ResultKind: 0), CancellationToken.None);
        Assert.True(simple.IsSuccess);
        Assert.Single(simple.Value!);
        Assert.Equal(101, simple.Value![0].PatientTestId);

        var culture = await handler.Handle(new GetResultWorklistQuery(ResultKind: 2), CancellationToken.None);
        Assert.True(culture.IsSuccess);
        Assert.Single(culture.Value!);
        Assert.Equal(103, culture.Value![0].PatientTestId);
        Assert.True(culture.Value![0].IsCultureType);
    }

    [Fact]
    public async Task AggregateStatus_Present_And_Correct_ForMixedStagePatient()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "Mixed"));
        AddCatalogTest(db, 10, "CBC");
        AddCatalogTest(db, 11, "Glucose");
        var reviewPending = MakeRow(101, 1, 10);
        reviewPending.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        var printPending = MakeRow(102, 1, 11);
        printPending.EnterResult("6", ResultFlag.Normal, 1, DateTime.UtcNow);
        printPending.MarkReviewed(1, DateTime.UtcNow);
        db.PatientTests.Add(reviewPending);
        db.PatientTests.Add(printPending);

        var handler = new GetResultWorklistQueryHandler(db);
        var result = await handler.Handle(new GetResultWorklistQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        // Min stage is review-pending (stage 2) → S3.
        Assert.All(result.Value!, item => Assert.Equal(3, item.AggregateStatus));
    }

    [Fact]
    public async Task Ordered_By_RegistrationDateUtc()
    {
        var db = new FakeApplicationDbContext();
        var baseDay = DateTime.UtcNow.Date.AddHours(10);
        db.Patients.Add(MakePatient(1, "Late", baseDay.AddHours(2)));
        db.Patients.Add(MakePatient(2, "Early", baseDay));
        AddCatalogTest(db, 10, "CBC");
        db.PatientTests.Add(MakeRow(101, 1, 10));
        db.PatientTests.Add(MakeRow(102, 2, 10));

        var handler = new GetResultWorklistQueryHandler(db);
        var result = await handler.Handle(new GetResultWorklistQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal(2, result.Value![0].PatientId);
        Assert.Equal(1, result.Value![1].PatientId);
    }
}
