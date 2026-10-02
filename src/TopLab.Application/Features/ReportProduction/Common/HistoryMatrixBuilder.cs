using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Common;

/// <summary>W-02 S12 (WP-10): one CBC-matrix cell.</summary>
public sealed record HistoryMatrixCell(
    string AnalyteName,
    string? ResultValue,
    string? Unit,
    int? Flag,
    string? LowComment);

/// <summary>W-02 S12 (WP-10): one matrix row = one date × the requested analytes.</summary>
public sealed record HistoryMatrixRow(
    DateOnly Date,
    IReadOnlyList<HistoryMatrixCell> Cells);

/// <summary>
/// W-02 S12 (WP-10): pivots profile-result items into date rows × analyte columns.
/// Pure function over already-loaded items — no database access, no N+1 by construction.
/// </summary>
public static class HistoryMatrixBuilder
{
    public static IReadOnlyList<HistoryMatrixRow> Pivot(
        IReadOnlyList<(DateOnly Date, string AnalyteName, string? ResultValue, string? Unit, int? Flag, string? LowComment)> points)
    {
        return points
            .GroupBy(p => p.Date)
            .OrderBy(g => g.Key)
            .Select(g => new HistoryMatrixRow(
                g.Key,
                g.OrderBy(p => p.AnalyteName, StringComparer.Ordinal)
                    .Select(p => new HistoryMatrixCell(
                        p.AnalyteName, p.ResultValue, p.Unit, p.Flag, p.LowComment))
                    .ToList()))
            .ToList();
    }
}
