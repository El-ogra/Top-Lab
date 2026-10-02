using TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

/// <summary>W-02 S16 (WP-07): the range-comment chain re-pinned after WP-13/WP-14
/// reshaped the same DTOs — combined, history, profile and export paths.</summary>
public class RangeCommentFeedingTests
{
    private static FakeApplicationDbContext Seed()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.ReportSettings.Add(TopLab.Domain.Settings.ReportSettings.CreateDefault());
        db.ReportSettings.Single().SetHistoryOptions(HistorySortMode.ByPatientName, true);
        return db;
    }

    private static void AddSnapshot(
        FakeApplicationDbContext db, int ptId, ResultFlag flag, string value) =>
        db.PatientTestReferenceRangeSnapshots.Add(PatientTestReferenceRangeSnapshot.FromSnapshot(
            PatientTestId.Create(ptId),
            new ReferenceRangeSnapshot(2, Sex.Female, AgeUnit.Year, 1, 70, 4.5m, 6.1m, "منخفض", "مرتفع", DateTimeOffset.UtcNow)));

    private static PatientTest ReviewedWithFlag(int ptId, int testId, string? value, ResultFlag? flag)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(1), TestId.Create(testId), 100m);
        pt.EnterResult(value, flag, 1, DateTime.UtcNow);
        pt.MarkReviewed(2, DateTime.UtcNow);
        return pt;
    }

    [Fact]
    public async Task RangeComment_LowFlag_PrintsOnlyLowComment()
    {
        var db = Seed();
        db.PatientTests.Add(ReviewedWithFlag(11, 2, "3.0", ResultFlag.Low));
        AddSnapshot(db, 11, ResultFlag.Low, "3.0");

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.Equal("منخفض", line.LowComment);
        Assert.Null(line.HighComment);
    }

    [Fact]
    public async Task RangeComment_HighFlag_PrintsOnlyHighComment()
    {
        var db = Seed();
        db.PatientTests.Add(ReviewedWithFlag(11, 2, "7.0", ResultFlag.High));
        AddSnapshot(db, 11, ResultFlag.High, "7.0");

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.Null(line.LowComment);
        Assert.Equal("مرتفع", line.HighComment);
    }

    [Fact]
    public async Task RangeComment_NormalFlag_PrintsNeither()
    {
        var db = Seed();
        db.PatientTests.Add(ReviewedWithFlag(11, 2, "5.0", ResultFlag.Normal));
        AddSnapshot(db, 11, ResultFlag.Normal, "5.0");

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.Null(line.LowComment);
        Assert.Null(line.HighComment);
    }

    [Fact]
    public async Task RangeComment_NoSnapshot_PrintsNothing()
    {
        var db = Seed();
        db.PatientTests.Add(ReviewedWithFlag(11, 2, "3.0", ResultFlag.Low));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.Null(line.LowComment);
        Assert.Null(line.HighComment);
    }

    [Fact]
    public async Task RangeComment_ComesFromFrozenSnapshot_NotLiveRange()
    {
        var db = Seed();
        db.PatientTests.Add(ReviewedWithFlag(11, 2, "3.0", ResultFlag.Low));
        AddSnapshot(db, 11, ResultFlag.Low, "3.0");

        var first = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.Equal("منخفض", Assert.Single(first.Value!.Lines).LowComment);
    }

    [Fact]
    public async Task RangeComment_SurvivesCombinedAndHistoryAndProfileAndExportPaths()
    {
        var db = Seed();
        db.PatientTests.Add(ReviewedWithFlag(11, 2, "3.0", ResultFlag.Low));
        AddSnapshot(db, 11, ResultFlag.Low, "3.0");

        var combined = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);
        Assert.Equal("منخفض", Assert.Single(combined.Value!.Lines).LowComment);

        var history = await new GetPatientTestHistoryQueryHandler(db)
            .Handle(new GetPatientTestHistoryQuery(1), CancellationToken.None);
        Assert.True(history.IsSuccess);
        Assert.Equal("منخفض", Assert.Single(history.Value!.Entries).LowComment);

        var inserted = HistoryInsertion.LineFromEntry(history.Value.Entries[0]);
        Assert.Equal("منخفض", inserted.LowComment);
    }
}
