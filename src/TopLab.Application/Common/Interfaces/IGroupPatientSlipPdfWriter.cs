using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Renders a <see cref="GroupPatientSlipBatchDto"/> batch — one independently
/// archivable slip per patient, one page per patient — to a PDF file at the
/// caller-supplied absolute path (Phase 6, GAP-013).
/// Never overwrites: throws when the target file already exists
/// (defense-in-depth, mirrors the lab-order/receipt writers).
/// Generation/write failures throw and are mapped by the printing service to
/// <c>Error.Unexpected</c>.
/// </summary>
public interface IGroupPatientSlipPdfWriter
{
    Task WritePdfAsync(
        string absolutePath,
        GroupPatientSlipBatchDto batch,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default);
}
