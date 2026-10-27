using TopLab.Application.Features.Statistics.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// GAP-18 (5.6.2.3) — renders the on-screen census report to a PDF file at the
/// caller-supplied absolute path. Never overwrites (existing project convention).
///
/// This port is deliberately **separate from <c>IBandedResultMonitorPdfWriter</c>** and the
/// other writers, following the same precedent (C-6, AS-6): it renders a distinct
/// document. It is **not** routed through <c>PrinterAssignment</c> — so no
/// <c>PrinterOutputType</c> value and no migration is needed.
///
/// The port lives in Application so Presentation depends only on the abstraction and
/// never names an Infrastructure type (SD-10, ADR-0005).
/// </summary>
public interface IStatisticsReportPdfWriter
{
    Task WritePdfAsync(
        string absolutePath,
        StatisticsReportModel report,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default);
}