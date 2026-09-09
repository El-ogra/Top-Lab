using TopLab.Application.Features.ProfileResults.Queries.GetProfileReport;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ProfileResults;

public class GetProfileReportQueryHandlerTests
{
    private static (FakeApplicationDbContext Db, PatientTest Pt) SeedReportScenario(
        decimal bandMin = 1m,
        decimal bandMax = 5m)
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1, bandMin, bandMax);
        ProfileResultsSeed.AddProfile(db, 10, 1);
        var pt = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);

        var item = ProfileResultItem.Create(
            ProfileResultItemId.Create(501),
            pt.Id,
            AnalyteId.Create(1),
            "4.2",
            "mg",
            null,
            isVerified: true);
        db.ProfileResultItems.Add(item);
        db.ProfileResultItemReferenceRangeSnapshots.Add(
            ProfileResultItemReferenceRangeSnapshot.Create(
                item.Id,
                item.AnalyteId,
                null,
                AgeUnit.Year,
                0,
                100,
                bandMin,
                bandMax,
                null,
                null,
                new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc)));
        return (db, pt);
    }

    [Fact]
    public async Task Report_RendersFrozenRanges_AndLifecycleHeader()
    {
        var (db, pt) = SeedReportScenario();
        pt.EnterResult(null, null, 1, DateTime.UtcNow, "ملاحظة التقرير");
        pt.MarkReviewed(1, DateTime.UtcNow);
        pt.MarkPrinted(1, DateTime.UtcNow);

        var result = await new GetProfileReportQueryHandler(db).Handle(new GetProfileReportQuery(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal("ملاحظة التقرير", dto.Comment);
        Assert.True(dto.IsReviewed);
        Assert.True(dto.IsPrinted);
        var line = dto.Lines.Single();
        Assert.Equal("4.2", line.ResultValue);
        Assert.Equal("mg", line.Unit);
        Assert.NotNull(line.FrozenRange);
        Assert.Equal(1m, line.FrozenRange!.MinValue);
        Assert.Equal(5m, line.FrozenRange.MaxValue);
        Assert.Equal("Report A1", line.FrozenRange.AnalyteName);
    }

    [Fact]
    public async Task Report_ReprintAfterRangeChange_StillShowsFrozenValues()
    {
        var (db, pt) = SeedReportScenario(bandMin: 1m, bandMax: 5m);
        pt.EnterResult(null, null, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        pt.MarkPrinted(1, DateTime.UtcNow);

        var changed = db.AnalyteReferenceRangeBands.Single();
        db.AnalyteReferenceRangeBands.Remove(changed);
        db.AnalyteReferenceRangeBands.Add(AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(changed.Id.Value + 50000),
            changed.AnalyteReferenceRangeId,
            AgeUnit.Year,
            0,
            100,
            9m,
            11m,
            null));

        var result = await new GetProfileReportQueryHandler(db).Handle(new GetProfileReportQuery(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = result.Value!.Lines.Single();
        Assert.Equal(1m, line.FrozenRange!.MinValue);
        Assert.Equal(5m, line.FrozenRange.MaxValue);
    }

    [Fact]
    public async Task Report_MissingTest_NotFound()
    {
        var db = new FakeApplicationDbContext();

        var result = await new GetProfileReportQueryHandler(db).Handle(new GetProfileReportQuery(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }
}