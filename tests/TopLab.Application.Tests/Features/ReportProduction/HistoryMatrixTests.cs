using TopLab.Application.Features.ReportProduction.Common;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

/// <summary>W-02 S12 (WP-10): CBC matrix pivot — dates × analytes.</summary>
public class HistoryMatrixTests
{
    [Fact]
    public void HistoryMatrix_PivotsAnalytesByDate()
    {
        var rows = HistoryMatrixBuilder.Pivot(new (DateOnly, string, string?, string?, int?, string?)[]
        {
            (new DateOnly(2026, 1, 1), "Hb", "13.5", "g/dL", (int?)0, (string?)"منخفض"),
            (new DateOnly(2026, 1, 1), "WBC", "6.1", "10^3", null, null),
            (new DateOnly(2026, 2, 1), "Hb", "14.0", "g/dL", null, null)
        });

        Assert.Equal(2, rows.Count);
        Assert.Equal(new DateOnly(2026, 1, 1), rows[0].Date);
        Assert.Equal(2, rows[0].Cells.Count);
        Assert.Equal("Hb", rows[0].Cells[0].AnalyteName);
    }

    [Fact]
    public void HistoryMatrix_GroupsSameDateIntoOneRow()
    {
        var rows = HistoryMatrixBuilder.Pivot(new (DateOnly, string, string?, string?, int?, string?)[]
        {
            (new DateOnly(2026, 1, 1), "B", "1", null, (int?)null, (string?)null),
            (new DateOnly(2026, 1, 1), "A", "2", null, (int?)null, (string?)null)
        });

        var single = Assert.Single(rows);
        Assert.Equal(new[] { "A", "B" }, single.Cells.Select(c => c.AnalyteName));
    }

    [Fact]
    public void HistoryMatrix_HandlesMissingAnalyteCell()
    {
        var rows = HistoryMatrixBuilder.Pivot(new (DateOnly, string, string?, string?, int?, string?)[]
        {
            (new DateOnly(2026, 1, 1), "Hb", "13.5", "g/dL", (int?)null, (string?)null)
        });

        Assert.Equal("13.5", Assert.Single(Assert.Single(rows).Cells).ResultValue);
    }

    [Fact]
    public void HistoryMatrix_EmptyHistory_ProducesEmptyMatrix()
    {
        Assert.Empty(HistoryMatrixBuilder.Pivot(
            Array.Empty<(DateOnly, string, string?, string?, int?, string?)>()));
    }
}
