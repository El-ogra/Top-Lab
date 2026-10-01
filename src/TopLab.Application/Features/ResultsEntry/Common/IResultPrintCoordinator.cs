using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.ResultsEntry.Common;

/// <summary>
/// Which report shape a single patient test should be printed as (W-02 S4, WP-06).
/// </summary>
public enum ResultPrintKind
{
    ProfileReport,
    CultureReport,
    SimpleResult,
    BlankReport
}

/// <summary>
/// Outcome of one honest print attempt.
/// <para>
/// **AD-1 (owner decision, 2026-10-01): there is deliberately no <c>PdfPath</c> here.**
/// <c>IReportPrintingService.PrintReportAsync</c> returns a bare <c>Result</c> and
/// <c>ReportPrintingService</c> writes the PDF to the OS temp directory and discards the path,
/// so no port in this codebase can report it. Rather than widen a shared port used by three
/// shipped print handlers, the outcome reports whether the sheet actually printed. A UI that
/// says "printed" now means a printer really received a page.
/// </para>
/// </summary>
public sealed record ResultPrintOutcome(
    int PatientTestId,
    ResultPrintKind Kind,
    bool Printed,
    string? ErrorMessage);

/// <summary>
/// One honest print path for the result-entry screens and bulk print (W-02 S4, WP-06).
/// <para>
/// The contract is strict and ordered: <b>build, then print, and never mark</b>.
/// SD-1 (decision 2 = b1) forbids <c>MarkPrinted</c> anywhere in this path — it prevents
/// future writes to <c>IsPrinted</c>/<c>PrintCount</c> while the columns stay mapped, and it
/// does not retro-zero a row that was already printed.
/// </para>
/// </summary>
public interface IResultPrintCoordinator
{
    Task<ResultPrintOutcome> PrintAsync(
        int patientTestId, ResultPrintKind kind, CancellationToken ct = default);
}
