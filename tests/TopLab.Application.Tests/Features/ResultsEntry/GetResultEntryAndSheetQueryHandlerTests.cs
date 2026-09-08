using TopLab.Application.Features.ResultsEntry.Queries.GetPatientResultSheet;
using TopLab.Application.Features.ResultsEntry.Queries.GetResultEntry;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public class GetResultEntryQueryHandlerTests
{
    private static Patient MakePatient(int id, Sex sex = Sex.Male, int age = 30, AgeUnit unit = AgeUnit.Year)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", sex, age, unit, DateTime.UtcNow);
    }

    private static void AddCatalogTest(FakeApplicationDbContext db, int id, string name)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), name, name, name, $"T{id}", 30, 100m));
    }

    [Fact]
    public async Task Returns_Ranges_And_NullFrozen_When_NoSnapshot()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, Sex.Female, 25));
        AddCatalogTest(db, 10, "CBC");
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(10), AgeUnit.Year, 0, 100, 4m, 10m));
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var handler = new GetResultEntryQueryHandler(db);
        var result = await handler.Handle(new GetResultEntryQuery(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.ReferenceRanges);
        Assert.Null(result.Value.FrozenRange);
        Assert.Equal("Female", result.Value.PatientSex);
        Assert.Equal("Year", result.Value.PatientAgeUnit);
        Assert.Equal(25, result.Value.PatientAgeValue);
    }

    [Fact]
    public async Task FrozenRange_Preferred_When_SnapshotPresent()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddCatalogTest(db, 10, "CBC");
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(10), AgeUnit.Year, 0, 100, 4m, 10m));
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        db.PatientTests.Add(pt);
        var snapshot = new ReferenceRangeSnapshot(10, Sex.Male, AgeUnit.Year, 0, 50, 1m, 2m, "lo", "hi", new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero));
        db.PatientTestReferenceRangeSnapshots.Add(PatientTestReferenceRangeSnapshot.FromSnapshot(PatientTestId.Create(101), snapshot));

        var handler = new GetResultEntryQueryHandler(db);
        var result = await handler.Handle(new GetResultEntryQuery(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.FrozenRange);
        Assert.Equal(1m, result.Value.FrozenRange!.MinValue);
        Assert.Equal(2m, result.Value.FrozenRange.MaxValue);
        Assert.Equal("Male", result.Value.FrozenRange.Sex);
    }

    [Fact]
    public async Task MissingTest_Returns_NotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetResultEntryQueryHandler(db);
        var result = await handler.Handle(new GetResultEntryQuery(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public async Task SoftDeletedPatient_Returns_NotFound()
    {
        var db = new FakeApplicationDbContext();
        var patient = MakePatient(1);
        patient.SoftDelete();
        db.Patients.Add(patient);
        AddCatalogTest(db, 10, "CBC");
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new GetResultEntryQueryHandler(db);
        var result = await handler.Handle(new GetResultEntryQuery(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }
}

public class GetPatientResultSheetQueryHandlerTests
{
    private static Patient MakePatient(int id, string name = "Sheet")
    {
        return Patient.Create(PatientId.Create(id), name, Sex.Male, 40, AgeUnit.Year, DateTime.UtcNow);
    }

    private static void AddCatalogTest(FakeApplicationDbContext db, int id, string name)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), name, name, name, $"T{id}", 30, 100m));
    }

    [Fact]
    public async Task Returns_AllLines_WithFrozenRanges()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddCatalogTest(db, 10, "CBC");
        AddCatalogTest(db, 11, "Glucose");
        var entered = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        entered.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow, "n1");
        var pending = PatientTest.Create(PatientTestId.Create(102), PatientId.Create(1), TestId.Create(11), 50m);
        db.PatientTests.Add(entered);
        db.PatientTests.Add(pending);
        var snapshot = new ReferenceRangeSnapshot(10, null, AgeUnit.Year, 0, 100, 4m, 10m, null, null, new DateTimeOffset(2026, 9, 8, 0, 0, 0, TimeSpan.Zero));
        db.PatientTestReferenceRangeSnapshots.Add(PatientTestReferenceRangeSnapshot.FromSnapshot(PatientTestId.Create(101), snapshot));

        var handler = new GetPatientResultSheetQueryHandler(db);
        var result = await handler.Handle(new GetPatientResultSheetQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Lines.Count);
        Assert.Equal("CBC", result.Value.Lines[0].TestName);
        Assert.NotNull(result.Value.Lines[0].FrozenRange);
        Assert.Null(result.Value.Lines[1].FrozenRange);
        Assert.Equal("n1", result.Value.Lines[0].Notes);
    }

    [Fact]
    public async Task MissingOrDeletedPatient_Returns_NotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetPatientResultSheetQueryHandler(db);
        var missing = await handler.Handle(new GetPatientResultSheetQuery(99), CancellationToken.None);
        Assert.False(missing.IsSuccess);
        Assert.Equal("المريض غير موجود.", missing.Error!.Message);

        var patient = MakePatient(2);
        patient.SoftDelete();
        db.Patients.Add(patient);
        var deleted = await handler.Handle(new GetPatientResultSheetQuery(2), CancellationToken.None);
        Assert.False(deleted.IsSuccess);
        Assert.Equal("المريض غير موجود.", deleted.Error!.Message);
    }
}
