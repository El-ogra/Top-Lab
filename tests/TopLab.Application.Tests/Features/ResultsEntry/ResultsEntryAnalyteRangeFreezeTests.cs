using TopLab.Application.Features.ResultsEntry.Commands.EnterResult;
using TopLab.Application.Features.ResultsEntry.Commands.RefreshResultReferenceRange;
using TopLab.Application.Features.ResultsEntry.Queries.GetResultEntry;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public class ResultsEntryAnalyteRangeFreezeTests
{
    private static (FakeApplicationDbContext Db, FakeCurrentUserService User, FakeDateTimeProvider Clock, int AnalyteId)
        BuildEntered(int bandMin = 4)
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = 7 };
        var clock = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 9, 8, 12, 0, 0, DateTimeKind.Utc) };
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.Patients.Add(Patient.Create(PatientId.Create(1), "P1", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

        var analyte = Analyte.Create(AnalyteId.Create(50), "Sodium", "Sodium");
        var range = AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(51), analyte.Id);
        db.Analytes.Add(analyte);
        db.AnalyteReferenceRanges.Add(range);
        db.AnalyteReferenceRangeBands.Add(AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(60), range.Id, AgeUnit.Year, 0, 100, bandMin, 10m, null));

        var test = Test.Create(TestId.Create(10), "T10", "T10", "T10", "T10", 30, 100m, ResultKind.Simple);
        test.MapToAnalyte(analyte.Id);
        db.Tests.Add(test);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m));

        var enter = new EnterResultCommandHandler(db, user, clock);
        _ = enter.Handle(new EnterResultCommand(101, "12"), CancellationToken.None).GetAwaiter().GetResult();

        return (db, user, clock, 50);
    }

    [Fact]
    public void Entry_FreezesAnalyteBand_IntoSnapshot()
    {
        var (db, _, _, _) = BuildEntered(bandMin: 4);

        var snapshot = Assert.Single(db.PatientTestReferenceRangeSnapshots);
        Assert.Equal(4m, snapshot.MinValue);
        Assert.Equal(10m, snapshot.MaxValue);
        Assert.Equal(ResultFlag.High, db.PatientTests[0].ResultFlag);
    }

    [Fact]
    public void HistoricalFreeze_Unchanged_ByLaterLiveBandEdits()
    {
        var (db, _, _, analyteId) = BuildEntered(bandMin: 4);

        // Replace live bands (simulating future range maintenance).
        var range = db.AnalyteReferenceRanges.Single();
        foreach (var old in db.AnalyteReferenceRangeBands.ToList())
        {
            db.Remove(old);
        }

        db.Add(AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(99), range.Id, AgeUnit.Year, 0, 100, 99m, 100m, null));

        var snapshot = Assert.Single(db.PatientTestReferenceRangeSnapshots);
        Assert.Equal(4m, snapshot.MinValue);
        Assert.Equal(ResultFlag.High, db.PatientTests[0].ResultFlag);
    }

    [Fact]
    public async Task GetResultEntry_Ranges_ComeFromAnalyteBands_NotLegacyRows()
    {
        var (db, _, _, _) = BuildEntered(bandMin: 4);
        // Legacy rows differ; mapped test must surface band ranges only.
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(10), AgeUnit.Year, 0, 100, 1m, 2m));

        var handler = new GetResultEntryQueryHandler(db);
        var result = await handler.Handle(new GetResultEntryQuery(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal(4m, dto.ReferenceRanges.Single().MinValue);
        Assert.Equal(10m, dto.ReferenceRanges.Single().MaxValue);
        Assert.Equal(4m, dto.FrozenRange!.MinValue);
    }

    [Fact]
    public async Task Refresh_ReselectsCurrentBands_AndRecomputesFlag()
    {
        var (db, _, _, _) = BuildEntered(bandMin: 4);

        var range = db.AnalyteReferenceRanges.Single();
        foreach (var old in db.AnalyteReferenceRangeBands.ToList())
        {
            db.Remove(old);
        }

        db.Add(AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(99), range.Id, AgeUnit.Year, 0, 100, 99m, 100m, null));

        var handler = new RefreshResultReferenceRangeCommandHandler(db);
        var result = await handler.Handle(new RefreshResultReferenceRangeCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var snapshot = Assert.Single(db.PatientTestReferenceRangeSnapshots);
        Assert.Equal(99m, snapshot.MinValue);
        // 12 is below the new 99..100 band.
        Assert.Equal(ResultFlag.Low, db.PatientTests[0].ResultFlag);
    }
}