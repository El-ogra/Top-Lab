using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureResults.Common;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

/// <summary>
/// W-02 S4 (WP-06): the coordinator must build, then print, and never mark.
/// Uses the existing hand-rolled <see cref="FakeReportPrintingService"/> — no mocking library.
/// </summary>
public class ResultPrintCoordinatorTests
{
    /// <summary>AD-1 supersedes the plan's ReturnsPdfPath item: no port can report a path.</summary>
    [Fact]
    public async Task ResultPrintCoordinator_BuildSucceedsPrintSucceeds_ReturnsPrintedTrue()
    {
        var printing = new FakeReportPrintingService();
        var coordinator = new ResultPrintCoordinator(
            new FakeSender().WithCultureReport(10, 1).WithCombinedReport(1, 10, Combined()), printing);

        var outcome = await coordinator.PrintAsync(10, ResultPrintKind.SimpleResult);

        Assert.True(outcome.Printed);
        Assert.Null(outcome.ErrorMessage);
        Assert.Equal(10, outcome.PatientTestId);
        Assert.Single(printing.Tokens);
    }

    /// <summary>The decisive honesty test: a printer failure must not be reported as success.</summary>
    [Fact]
    public async Task ResultPrintCoordinator_PrintFails_ReturnsPrintedFalseAndNoMarking()
    {
        var printing = new FakeReportPrintingService
        {
            NextResult = Result.Failure(Error.Unexpected("تعذر طباعة التقرير."))
        };
        var coordinator = new ResultPrintCoordinator(
            new FakeSender().WithCultureReport(10, 1).WithCombinedReport(1, 10, Combined()), printing);

        var outcome = await coordinator.PrintAsync(10, ResultPrintKind.SimpleResult);

        Assert.False(outcome.Printed);
        Assert.Equal("تعذر طباعة التقرير.", outcome.ErrorMessage);
        // The failure is surfaced verbatim, not replaced with new wording.
        Assert.Single(printing.Tokens);
    }

    [Fact]
    public async Task ResultPrintCoordinator_BuildFails_DoesNotCallPrinting()
    {
        var printing = new FakeReportPrintingService();
        var coordinator = new ResultPrintCoordinator(
            new FakeSender().WithCultureFailure(10).WithProfileFailure(10), printing);

        var outcome = await coordinator.PrintAsync(10, ResultPrintKind.SimpleResult);

        Assert.False(outcome.Printed);
        Assert.Empty(printing.Tokens);
    }

    [Fact]
    public async Task ResultPrintCoordinator_ReturnsArabicErrorMessage()
    {
        var printing = new FakeReportPrintingService();
        var coordinator = new ResultPrintCoordinator(
            new FakeSender().WithCultureFailure(10).WithProfileFailure(10), printing);

        var outcome = await coordinator.PrintAsync(10, ResultPrintKind.SimpleResult);

        Assert.False(outcome.Printed);
        Assert.False(string.IsNullOrWhiteSpace(outcome.ErrorMessage));
        Assert.Equal("التحليل غير موجود", outcome.ErrorMessage);
    }

    [Fact]
    public async Task ResultPrintCoordinator_CultureReport_BuildsThroughCultureQuery()
    {
        var printing = new FakeReportPrintingService();
        var sender = new FakeSender().WithCultureReport(10, 1).WithCombinedReport(1, 10, Combined());
        var coordinator = new ResultPrintCoordinator(sender, printing);

        var outcome = await coordinator.PrintAsync(10, ResultPrintKind.CultureReport);

        Assert.True(outcome.Printed);
        Assert.Equal(ResultPrintKind.CultureReport, outcome.Kind);
        Assert.Single(printing.Tokens);
    }

    [Fact]
    public async Task ResultPrintCoordinator_ProfileReport_BuildsThroughGetProfileReportQuery()
    {
        var printing = new FakeReportPrintingService();
        var sender = new FakeSender().WithCultureReport(10, 1).WithProfileReport(10, 1);
        var coordinator = new ResultPrintCoordinator(sender, printing);

        var outcome = await coordinator.PrintAsync(10, ResultPrintKind.ProfileReport);

        Assert.True(outcome.Printed);
        Assert.Single(printing.Tokens);
    }

    [Fact]
    public async Task ResultPrintCoordinator_BlankReport_PrintsWithoutResultLines()
    {
        var printing = new FakeReportPrintingService();
        var sender = new FakeSender().WithCultureReport(10, 1)
            .WithBlankReport(new BlankReportDto(1, "P", null, "Male", 30, "Year", null, null));
        var coordinator = new ResultPrintCoordinator(sender, printing);

        var outcome = await coordinator.PrintAsync(10, ResultPrintKind.BlankReport);

        Assert.True(outcome.Printed);
        Assert.Single(printing.Tokens);
    }

    [Fact]
    public async Task ResultPrintCoordinator_RejectsNonPositivePatientTestId()
    {
        var printing = new FakeReportPrintingService();
        var coordinator = new ResultPrintCoordinator(new FakeSender(), printing);

        var outcome = await coordinator.PrintAsync(0, ResultPrintKind.SimpleResult);

        Assert.False(outcome.Printed);
        Assert.Empty(printing.Tokens);
    }

    private static CombinedReportDto Combined() => new(1, "P", null,
        [new CombinedReportLineDto(10, 1, "Test", "C", 0, "1.0", null, null, [], null)]);
}
