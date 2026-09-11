namespace TopLab.Application.Features.WorkSheets.Common;

/// <summary>
/// Permission codes consumed by the M-11 work-sheet queries. Worksheet generation
/// is gated on PRINT_WORKSHEET (FR-M17-004 item 8); the period test-count
/// classification lives inside the M11 screen (S-33), so it shares the worksheet
/// grant rather than STATISTICS.
/// </summary>
public static class WorkSheetsAccessPolicy
{
    public const string PrintWorksheet = "PRINT_WORKSHEET";
}
