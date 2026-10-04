using TopLab.Application.Features.Statistics.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// R-F05 (BR-F05-13) — renders the banded result monitor grid to a PDF file at the
/// caller-supplied absolute path. Never overwrites (BR-F05-14).
///
/// This port is deliberately **separate from <c>IPriceListPdfWriter</c> and
/// <c>ICustomGroupPdfWriter</c>**, following the same precedent (C-6, AS-6): it is a
/// **multi-patient** grid with no single patient row, so it could not travel through
/// <c>ReportDocumentContent</c> or <c>ReportPrintEnvelope</c> without a fake patient.
/// It shares no class and no DTO with the other two writers, and is **not** routed
/// through <c>PrinterAssignment</c> — so no <c>PrinterOutputType</c> value and no
/// migration is needed.
///
/// The port lives in Application so Presentation depends only on the abstraction and
/// never names an Infrastructure type (SD-10, ADR-0005).
/// </summary>
public interface IBandedResultMonitorPdfWriter
{
    Task WritePdfAsync(
        string absolutePath,
        BandedResultMonitorDto monitor,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default);
}