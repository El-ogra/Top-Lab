using TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestCommentsForResults;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

/// <summary>W-02 S8 (WP-13): off-lab note, aggregated test comments, group sub-title data.</summary>
public class CombinedReportOptionsTests
{
    [Fact]
    public async Task BuildCombinedReport_OutsideLab_CarriesNote()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        var pt = PatientTest.Create(
            PatientTestId.Create(11), PatientId.Create(1), TestId.Create(2), 100m,
            isTakenOutsideLab: true);
        pt.EnterResult("5.5", null, 1, DateTime.UtcNow);
        pt.MarkReviewed(2, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(Assert.Single(result.Value!.Lines).IsTakenOutsideLab);
    }

    [Fact]
    public async Task BuildCombinedReport_InLab_CarriesNoNote()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(GetCombinableTestsQueryHandlerTests.Reviewed(11, testId: 2, value: "5.5"));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.False(line.IsTakenOutsideLab);
        Assert.True(line.TestComments is null || line.TestComments.Count == 0);
    }

    [Fact]
    public async Task BuildCombinedReport_AggregatesTestCommentsInOneQuery()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(GetCombinableTestsQueryHandlerTests.Reviewed(11, testId: 2, value: "5.5"));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(1), TestId.Create(2), "صائم 8 ساعات"));
        db.TestComments.Add(TestComment.Create(TestCommentId.Create(2), TestId.Create(2), "يُعاد عند الشك"));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var comments = Assert.Single(result.Value!.Lines).TestComments;
        Assert.NotNull(comments);
        Assert.Equal(new[] { "صائم 8 ساعات", "يُعاد عند الشك" }, comments);
    }

    [Fact]
    public void InsertedComment_AppearsInReportPayload()
    {
        var entry = new HistoryEntryDto(
            11, 1, 2, "Glucose", "GLU", 0, "5.5", null, true,
            DateTime.UtcNow, null, null, null, false, new[] { "صائم" });

        var line = HistoryInsertion.LineFromEntry(entry);

        Assert.Equal(new[] { "صائم" }, line.TestComments);
        Assert.False(line.IsTakenOutsideLab);
    }

    [Fact]
    public async Task CommentPicker_DoesNotMutatePatientTest()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        var pt = GetCombinableTestsQueryHandlerTests.Reviewed(11, testId: 2, value: "5.5");
        db.PatientTests.Add(pt);
        var before = db.PatientTests.Count;

        var comments = await new GetTestCommentsForResultsQueryHandler(db)
            .Handle(
                new GetTestCommentsForResultsQuery(new[] { 2 }),
                CancellationToken.None);

        Assert.True(comments.IsSuccess);
        Assert.Equal(before, db.PatientTests.Count);
    }
}
