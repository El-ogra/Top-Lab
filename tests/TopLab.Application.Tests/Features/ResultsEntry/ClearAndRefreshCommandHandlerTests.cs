using TopLab.Application.Features.ResultsEntry.Commands.ClearResult;
using TopLab.Application.Features.ResultsEntry.Commands.RefreshResultReferenceRange;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public class ClearAndRefreshCommandHandlerTests
{
    private static Patient MakePatient(int id = 1)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
    }

    private static void AddSimpleTest(FakeApplicationDbContext db, int id)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}", $"T{id}", 30, 100m, ResultKind.Simple));
    }

    [Fact]
    public async Task Clear_Deletes_Snapshot_And_Resets()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow, "n");
        db.PatientTests.Add(pt);
        var snap = new ReferenceRangeSnapshot(10, null, AgeUnit.Year, 0, 100, 1m, 2m, null, null, DateTimeOffset.UtcNow);
        db.PatientTestReferenceRangeSnapshots.Add(PatientTestReferenceRangeSnapshot.FromSnapshot(PatientTestId.Create(101), snap));

        var handler = new ClearResultCommandHandler(db);
        var result = await handler.Handle(new ClearResultCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(pt.ResultValue);
        Assert.Empty(db.PatientTestReferenceRangeSnapshots);
    }

    [Fact]
    public async Task Clear_Locked_Returns_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var handler = new ClearResultCommandHandler(db);
        var result = await handler.Handle(new ClearResultCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن مسح نتيجة معتمدة أو مطبوعة أو مسلمة.", result.Error!.Message);
    }

    [Fact]
    public async Task Refresh_Replaces_Snapshot_And_Recomputes_Flag()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(10), AgeUnit.Year, 0, 100, 4m, 10m));
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("12", ResultFlag.Normal, 1, DateTime.UtcNow);
        db.PatientTests.Add(pt);
        var stale = new ReferenceRangeSnapshot(10, null, AgeUnit.Year, 0, 100, 0m, 1m, null, null, DateTimeOffset.UtcNow);
        db.PatientTestReferenceRangeSnapshots.Add(PatientTestReferenceRangeSnapshot.FromSnapshot(PatientTestId.Create(101), stale));

        var handler = new RefreshResultReferenceRangeCommandHandler(db);
        var result = await handler.Handle(new RefreshResultReferenceRangeCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.PatientTestReferenceRangeSnapshots);
        Assert.Equal(4m, db.PatientTestReferenceRangeSnapshots[0].MinValue);
        Assert.Equal(ResultFlag.High, pt.ResultFlag);
    }

    [Fact]
    public async Task Refresh_OldValues_Persist_Until_Refresh()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        // Current catalog range is 4-10, but the frozen row still holds 0-1.
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(10), AgeUnit.Year, 0, 100, 4m, 10m));
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("0.5", ResultFlag.Normal, 1, DateTime.UtcNow);
        db.PatientTests.Add(pt);
        var stale = new ReferenceRangeSnapshot(10, null, AgeUnit.Year, 0, 100, 0m, 1m, null, null, DateTimeOffset.UtcNow);
        db.PatientTestReferenceRangeSnapshots.Add(PatientTestReferenceRangeSnapshot.FromSnapshot(PatientTestId.Create(101), stale));

        Assert.Equal(0m, db.PatientTestReferenceRangeSnapshots[0].MinValue);

        var handler = new RefreshResultReferenceRangeCommandHandler(db);
        var result = await handler.Handle(new RefreshResultReferenceRangeCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4m, db.PatientTestReferenceRangeSnapshots[0].MinValue);
    }

    [Fact]
    public async Task Refresh_Reviewed_Rejected()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(10), AgeUnit.Year, 0, 100, 4m, 10m));
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var handler = new RefreshResultReferenceRangeCommandHandler(db);
        var result = await handler.Handle(new RefreshResultReferenceRangeCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("النتيجة معتمدة؛ ألغِ الاعتماد أولاً.", result.Error!.Message);
    }

    [Fact]
    public async Task Refresh_NoMatch_Deletes_Snapshot()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        db.PatientTests.Add(pt);
        var stale = new ReferenceRangeSnapshot(10, null, AgeUnit.Year, 0, 100, 1m, 2m, null, null, DateTimeOffset.UtcNow);
        db.PatientTestReferenceRangeSnapshots.Add(PatientTestReferenceRangeSnapshot.FromSnapshot(PatientTestId.Create(101), stale));

        var handler = new RefreshResultReferenceRangeCommandHandler(db);
        var result = await handler.Handle(new RefreshResultReferenceRangeCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(db.PatientTestReferenceRangeSnapshots);
        Assert.Null(pt.ResultFlag);
    }
}
