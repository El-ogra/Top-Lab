using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Renders a <see cref="LabOrderPrintEnvelope"/> lab-order slip to a PDF file at the
/// caller-supplied absolute path. Never overwrites: throws when the target
/// file already exists (defense-in-depth, mirrors the receipt writer).
/// Generation/write failures throw and are mapped by the printing service to
/// <c>Error.Unexpected</c>.
/// </summary>
public interface ILabOrderPdfWriter
{
    Task WritePdfAsync(
        string absolutePath,
        LabOrderDto order,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default);
}
