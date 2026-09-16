using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Settings;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Renders a <see cref="ReceiptPrintEnvelope"/> receipt to a PDF file at the
/// caller-supplied absolute path, honouring the print settings snapshot read at
/// print time. Never overwrites: uses <c>FileMode.CreateNew</c> (defense-in-depth,
/// mirrors <c>PatientReportPdfExporter</c>). Generation/write failures throw and
/// are mapped by the printing service to <c>Error.Unexpected</c>.
/// </summary>
public interface IReceiptPdfWriter
{
    Task WritePdfAsync(
        string absolutePath,
        ReceiptDto receipt,
        ReceiptSettings receiptSettings,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default);
}
