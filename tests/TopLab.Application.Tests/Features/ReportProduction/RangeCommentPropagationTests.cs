using TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

/// <summary>WP-07 (bundled into WP-01): frozen range comments reach the report DTO.</summary>
public class RangeCommentPropagationTests
{
    private static PatientTestReferenceRangeSnapshot Snap(
        int patientTestId,
        decimal min,
        decimal max,
        string? low,
        string? high)
    {
        return PatientTestReferenceRangeSnapshot.FromSnapshot(
            PatientTestId.Create(patientTestId),
            new ReferenceRangeSnapshot(
                2, Sex.Female, AgeUnit.Year, 1, 70, min, max, low, high, DateTimeOffset.UtcNow));
    }

    private static PatientTest ReviewedWithFlag(int ptId, int testId, string value, ResultFlag flag)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(1), TestId.Create(testId), 100m);
        pt.EnterResult(value, flag, 1, DateTime.UtcNow);
        pt.MarkReviewed(2, DateTime.UtcNow);
        return pt;
    }

    [Fact]
    public async Task BuildCombinedReport_LowResult_CarriesLowComment()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(ReviewedWithFlag(11, testId: 2, "1.0", ResultFlag.Low));
        db.PatientTestReferenceRangeSnapshots.Add(Snap(11, 4.0m, 6.0m, "منخفض عن الحد", "مرتفع عن الحد"));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.Equal("منخفض عن الحد", line.LowComment);
        Assert.Null(line.HighComment);
    }

    [Fact]
    public async Task BuildCombinedReport_HighResult_CarriesHighComment()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(ReviewedWithFlag(11, testId: 2, "99", ResultFlag.High));
        db.PatientTestReferenceRangeSnapshots.Add(Snap(11, 4.0m, 6.0m, "منخفض عن الحد", "مرتفع عن الحد"));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.Null(line.LowComment);
        Assert.Equal("مرتفع عن الحد", line.HighComment);
    }

    [Fact]
    public async Task BuildCombinedReport_NormalResult_CarriesNoComment()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(GetCombinableTestsQueryHandlerTests.Reviewed(11, testId: 2, value: "5.0"));
        db.PatientTestReferenceRangeSnapshots.Add(Snap(11, 4.0m, 6.0m, "منخفض", "مرتفع"));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.Null(line.LowComment);
        Assert.Null(line.HighComment);
    }

    [Fact]
    public async Task BuildCombinedReport_UsesFrozenRangeNotLiveRange()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(ReviewedWithFlag(11, testId: 2, "1.0", ResultFlag.Low));
        // Frozen comment "snapshot-comment" must win over any live range text.
        db.PatientTestReferenceRangeSnapshots.Add(Snap(11, 4.0m, 6.0m, "snapshot-comment", "snapshot-high"));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.Equal("snapshot-comment", line.LowComment);
    }

    [Fact]
    public async Task BuildCombinedReport_ProfileLine_CarriesFrozenComments()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(GetCombinableTestsQueryHandlerTests.Reviewed(11, testId: 3, value: null));
        db.Analytes.Add(Analyte.Create(AnalyteId.Create(5), "Iron", "Iron"));
        db.ProfileResultItems.Add(ProfileResultItem.Create(
            ProfileResultItemId.Create(30), PatientTestId.Create(11), AnalyteId.Create(5), "70", "ug/dL"));
        db.ProfileResultItems.Add(ProfileResultItem.Create(
            ProfileResultItemId.Create(31), PatientTestId.Create(11), AnalyteId.Create(6), "250", "ug/dL"));
        db.ProfileResultItemReferenceRangeSnapshots.Add(ProfileResultItemReferenceRangeSnapshot.Create(
            ProfileResultItemId.Create(30), AnalyteId.Create(5), Sex.Male, AgeUnit.Year, 1, 70, 30m, 100m,
            "iron-low", "iron-high", DateTimeOffset.UtcNow));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        var profile = Assert.Single(line.ProfileLines, p => p.AnalyteId == 5);
        Assert.Equal("iron-low", profile.FrozenRange!.LowComment);
        Assert.Equal("iron-high", profile.FrozenRange.HighComment);
    }

    [Fact]
    public void GetPatientTestHistory_CarriesRangeComments()
    {
        var (low, high) = PatientHistoryReader.RangeComments(
            Snap(11, 4m, 6m, "hist-low", "hist-high"),
            ResultFlag.Low);
        Assert.Equal("hist-low", low);
        Assert.Null(high);

        (low, high) = PatientHistoryReader.RangeComments(
            Snap(11, 4m, 6m, "hist-low", "hist-high"),
            ResultFlag.High);
        Assert.Null(low);
        Assert.Equal("hist-high", high);

        (low, high) = PatientHistoryReader.RangeComments(
            Snap(11, 4m, 6m, "hist-low", "hist-high"),
            ResultFlag.Normal);
        Assert.Null(low);
        Assert.Null(high);
    }
}
