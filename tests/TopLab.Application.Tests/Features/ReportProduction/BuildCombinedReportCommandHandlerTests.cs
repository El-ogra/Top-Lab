using TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class BuildCombinedReportCommandHandlerTests
{
    [Fact]
    public async Task BuildsLinesInGivenOrder()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(Reviewed(11, testId: 2, value: "90"));
        db.PatientTests.Add(Reviewed(12, testId: 3, value: "1.5"));
        db.PatientTests.Add(Reviewed(13, testId: 4, value: "120"));
        db.Tests.Add(Test.Create(TestId.Create(4), "CBC", "CBC report", "CBC", "CBC", 1, 100m, ResultKind.Simple));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 13, 11, 12 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 13, 11, 12 }, result.Value!.Lines.Select(l => l.PatientTestId));
        Assert.Equal("Ali", result.Value.PatientFullName);
        Assert.Equal(0, result.Value.Lines[0].ResultKind);
    }

    [Fact]
    public async Task DuplicateIds_ReturnsTranslatedConflict()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(Reviewed(11, testId: 2, value: "90"));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11, 11 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن تكرار نفس التحليل في التقرير.", result.Error!.Message);
    }

    [Fact]
    public async Task UnreviewedLine_ReturnsTranslatedConflict()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(GetCombinableTestsQueryHandlerTests.EnteredOnly(11, testId: 2));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن إدراج نتيجة غير معتمدة في التقرير.", result.Error!.Message);
    }

    [Fact]
    public async Task UnknownId_ReturnsNotFound()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(Reviewed(11, testId: 2, value: "90"));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11, 99 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public async Task MissingPatient_ReturnsNotFound()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(42, new[] { 11 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task SimpleLine_RendersSnapshotRangeInvariant()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(Reviewed(11, testId: 2, value: "5.5"));
        db.PatientTestReferenceRangeSnapshots.Add(PatientTestReferenceRangeSnapshot.FromSnapshot(
            PatientTestId.Create(11),
            new ReferenceRangeSnapshot(2, Sex.Female, AgeUnit.Year, 1, 70, 4.5m, 6.1m, null, null, DateTimeOffset.UtcNow)));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.Equal("5.5", line.ResultValue);
        Assert.Equal("4.5 - 6.1", line.FrozenRangeText);
    }

    [Fact]
    public async Task SimpleLine_WithoutSnapshot_HasNullRangeText()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(Reviewed(11, testId: 2, value: "5.5"));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(Assert.Single(result.Value!.Lines).FrozenRangeText);
    }

    [Fact]
    public async Task ProfileLine_LoadsItemsAndFrozenRanges()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.PatientTests.Add(Reviewed(11, testId: 3, value: null));
        db.Analytes.Add(Analyte.Create(AnalyteId.Create(5), "Iron", "Iron (Ferritin)"));
        db.Analytes.Add(Analyte.Create(AnalyteId.Create(6), "TIBC", "TIBC"));
        db.ProfileResultItems.Add(ProfileResultItem.Create(ProfileResultItemId.Create(30), PatientTestId.Create(11), AnalyteId.Create(5), "70", "ug/dL"));
        db.ProfileResultItems.Add(ProfileResultItem.Create(ProfileResultItemId.Create(31), PatientTestId.Create(11), AnalyteId.Create(6), "250", "ug/dL"));
        db.ProfileResultItemReferenceRangeSnapshots.Add(ProfileResultItemReferenceRangeSnapshot.Create(
            ProfileResultItemId.Create(30), AnalyteId.Create(5), Sex.Male, AgeUnit.Year, 1, 70, 30m, 100m, null, null, DateTimeOffset.UtcNow));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.Equal(2, line.ProfileLines.Count);
        Assert.Equal("Iron (Ferritin)", line.ProfileLines[0].AnalyteName);
        Assert.Equal(30m, line.ProfileLines[0].FrozenRange!.MinValue);
        Assert.Equal(100m, line.ProfileLines[0].FrozenRange!.MaxValue);
        Assert.Null(line.ProfileLines[1].FrozenRange);
        Assert.Null(line.Culture);
    }

    [Fact]
    public async Task CultureLine_LoadsCultureSummary()
    {
        var db = GetCombinableTestsQueryHandlerTests.Seed();
        db.Tests.Add(Test.Create(TestId.Create(4), "CultureA", "Culture report", "Culture", "CULT4", 1, 100m, ResultKind.Culture, isCultureType: true));
        db.PatientTests.Add(Reviewed(11, testId: 4, value: null));
        db.CultureResults.Add(new CultureResult(PatientTestId.Create(11), "Urine", "E.coli", organismB: "Klebsiella"));

        var result = await new BuildCombinedReportCommandHandler(db)
            .Handle(new BuildCombinedReportCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.NotNull(line.Culture);
        Assert.Equal("Urine", line.Culture!.Sample);
        Assert.Equal("E.coli", line.Culture.OrganismA);
        Assert.Equal("Klebsiella", line.Culture.OrganismB);
        Assert.Empty(line.ProfileLines);
    }

    internal static PatientTest Reviewed(int ptId, int testId, string? value)
    {
        return GetCombinableTestsQueryHandlerTests.Reviewed(ptId, testId, value);
    }
}