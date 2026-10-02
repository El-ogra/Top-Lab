using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>W-02 S11 (WP-14): the microbiology block renders a real sensitivity
/// table plus a microscopy block — and never a commercial column.</summary>
public class CultureReportSectionTests
{
    private static TopLab.Infrastructure.Printing.ReportCultureSection Full() =>
        new(
            "Urine", "E.coli", null, null, "Aerobic", "10^5",
            "++", null, null, null, null, null, null, null, true,
            new[]
            {
                new TopLab.Infrastructure.Printing.ReportCultureSensitivityRow("Amoxicillin", "Sensitive", 18.5m, "Amoxicillin trihydrate"),
                new TopLab.Infrastructure.Printing.ReportCultureSensitivityRow("Ciprofloxacin", "Resistant", null, null)
            });

    [Fact]
    public void Report_PrintsSensitivityTableRows()
    {
        var grid = Full().BuildSensitivityGrid();

        Assert.Equal(2, grid.Count);
        Assert.Equal("Amoxicillin", grid[0][0]);
        Assert.Equal("Sensitive", grid[0][1]);
    }

    [Fact]
    public void Report_PrintsMicroscopyBlock()
    {
        var lines = Full().BuildMicroscopyLines();

        Assert.Contains(lines, l => l.Contains("صديدية"));
        Assert.Contains(lines, l => l.Contains("مباشر"));
    }

    [Fact]
    public void Report_MicroscopyBlock_OmittedWhenAllFieldsNull()
    {
        var section = new TopLab.Infrastructure.Printing.ReportCultureSection(
            "Urine", null, null, null, null, null);

        Assert.Empty(section.BuildMicroscopyLines());
        Assert.Empty(section.BuildSensitivityGrid());
    }

    [Fact]
    public void Report_SensitivityTable_OmittedWhenNoRows()
    {
        var section = new TopLab.Infrastructure.Printing.ReportCultureSection(
            "Urine", "E.coli", null, null, null, null);

        Assert.Empty(section.BuildSensitivityGrid());
        Assert.True(section.HasAnyContent);
    }

    [Fact]
    public void Report_ZoneFormattedWithInvariantDecimal()
    {
        var grid = Full().BuildSensitivityGrid();

        Assert.Equal("18.5", grid[0][2]);
        Assert.Equal(string.Empty, grid[1][2]);
    }

    [Fact]
    public void Report_SensitivityTable_OmitsCommercialColumn()
    {
        var grid = Full().BuildSensitivityGrid();

        Assert.All(grid, row => Assert.Equal(4, row.Count));
    }

    [Fact]
    public void ReportCultureSection_ExistingSixFields_Unchanged()
    {
        var section = new TopLab.Infrastructure.Printing.ReportCultureSection(
            "Urine", "E.coli", "Kleb", "Prot", "Aerobic", "10^5");

        var lines = section.BuildLines();

        Assert.Contains(lines, l => l.Contains("Urine"));
        Assert.Contains(lines, l => l.Contains("E.coli"));
        Assert.True(section.HasAnyContent);
    }

    [Fact]
    public void RangeComment_SurvivesExportPath()
    {
        var frozen = new TopLab.Application.Features.ResultsEntry.Common.FrozenRangeDto(
            2, null, "Year", 1, 70, 4.5m, 6.1m, "منخفض", "مرتفع", DateTimeOffset.UtcNow);
        var data = new TopLab.Application.Features.ResultsEntry.Common.PatientReportPdfData(
            1, "P", null,
            new[]
            {
                new TopLab.Application.Features.ResultsEntry.Common.PatientReportPdfLine(
                    11, "Glucose", "GLU", "3.0", 1, null, frozen,
                    Array.Empty<string>(), null)
            });

        var content = TopLab.Infrastructure.Printing.ReportContentBuilder.FromPatientExport(
            data,
            TopLab.Domain.Settings.ReportSettings.CreateDefault(),
            TopLab.Domain.Settings.SystemSettings.CreateDefault());

        var text = string.Join("\n", content.Sections.SelectMany(s => s.DisplayLines));
        Assert.Contains("منخفض", text, StringComparison.Ordinal);
    }
}
