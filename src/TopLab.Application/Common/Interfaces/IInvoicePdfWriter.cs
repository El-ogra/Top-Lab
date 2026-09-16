using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Renders an <see cref="InvoicePrintEnvelope"/> invoice to a PDF file at the
/// caller-supplied absolute path. Never overwrites: throws when the target
/// file already exists (defense-in-depth, mirrors the receipt writer).
/// Generation/write failures throw and are mapped by the printing service to
/// <c>Error.Unexpected</c>.
/// </summary>
public interface IInvoicePdfWriter
{
    Task WritePdfAsync(
        string absolutePath,
        InvoiceDto invoice,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default);
}
