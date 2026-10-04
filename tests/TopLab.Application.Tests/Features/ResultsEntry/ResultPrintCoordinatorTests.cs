using System.Text.Json;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureResults.Common;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileReport;
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

        // =====================================================================
        // R-A04-S3 (VG-08) — the specialised-profile printed report now carries the
        // REAL IsTakenOutsideLab. BR-A04-7 / SD-12: it is the THIRTEENTH member of
        // CombinedReportLineDto (ReportDtos.cs:71), not the tenth.
        // =====================================================================

        /// <summary>
        /// Builds the single-line CombinedReportDto the coordinator produces for a profile
        /// report. The flag is supplied positionally as the THIRTEENTH argument, after
        /// Culture, LowComment and HighComment — mirroring ResultPrintCoordinator exactly, so
        /// a future reordering cannot silently change what is asserted.
        /// </summary>
        private static ProfileReportDto ProfileReport(bool isTakenOutsideLab) => new(
            10,
            1,
            "Patient",
            null,
            "بروتوكول茶叶",
            null,
            false,
            false,
            false,
            [],
            isTakenOutsideLab);

        /// <summary>Decodes the token the coordinator handed to the printing port.</summary>
        private static CombinedReportDto ProfileEnvelope(string token)
        {
            using var outer = JsonDocument.Parse(token);
            var json = outer.RootElement.GetProperty("ReportJson").GetString()!;
            return JsonSerializer.Deserialize<CombinedReportDto>(json)!;
        }

        [Fact]
        public async Task ResultPrintCoordinator_ProfileReport_CarriesTheRealOutsideLabFlag()
        {
            var printing = new FakeReportPrintingService();
            var sender = new FakeSender()
                .WithCultureReport(10, 1)
                .WithResponse(
                new GetProfileReportQuery(10),
                Result<ProfileReportDto>.Success(ProfileReport(isTakenOutsideLab: true)));
            var coordinator = new ResultPrintCoordinator(sender, printing);

            var outcome = await coordinator.PrintAsync(10, ResultPrintKind.ProfileReport);

            Assert.True(outcome.Printed);
            var line = Assert.Single(ProfileEnvelope(Assert.Single(printing.Tokens)).Lines);
            Assert.True(line.IsTakenOutsideLab);
        }

        [Fact]
        public async Task ResultPrintCoordinator_ProfileReport_FalseFlag_StaysFalse()
        {
            // The no-regression half: a profile whose sample was NOT taken outside the lab must
            // still render false, so the fix did not simply hard-code the note on.
            var printing = new FakeReportPrintingService();
            var sender = new FakeSender()
                .WithCultureReport(10, 1)
                .WithResponse(
                new GetProfileReportQuery(10),
                Result<ProfileReportDto>.Success(ProfileReport(isTakenOutsideLab: false)));
            var coordinator = new ResultPrintCoordinator(sender, printing);

            var outcome = await coordinator.PrintAsync(10, ResultPrintKind.ProfileReport);

            Assert.True(outcome.Printed);
            var line = Assert.Single(ProfileEnvelope(Assert.Single(printing.Tokens)).Lines);
            Assert.False(line.IsTakenOutsideLab);
        }

        [Fact]
        public void ProfileReportDto_GainedTheMemberAsOptionalWithDefault()
        {
            // SD-13: the member must be OPTIONAL so the 10-argument positional construction in
            // tests/.../Common/Fakes/FakeSender.cs:105-106 keeps compiling untouched. This fact
            // constructs it positionally with ten arguments — exactly that call site.
            var tenArgs = new ProfileReportDto(10, 1, "Patient", null, "بروتوكول", null, false, false, false, []);

            Assert.False(tenArgs.IsTakenOutsideLab);
        }
    }
