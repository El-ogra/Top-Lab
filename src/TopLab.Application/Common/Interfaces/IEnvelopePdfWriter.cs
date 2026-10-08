using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Settings;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Renders an <see cref="EnvelopePrintEnvelope"/> envelope to a PDF file at the
/// caller-supplied absolute path, honouring the envelope settings snapshot read at
/// print time (top margin, header/footer words, per-item offsets, caption
/// suppression). Never overwrites: throws when the target file already exists
/// (defense-in-depth, mirrors <c>PatientReportPdfExporter</c>).
/// Generation/write failures throw and are mapped by the printing service to
/// <c>Error.Unexpected</c>.
/// </summary>
public interface IEnvelopePdfWriter
{
    Task WritePdfAsync(
        string absolutePath,
        EnvelopeDto envelope,
        EnvelopeSettings settings,
        IReadOnlyList<EnvelopePrintItemPosition> positions,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default);
}
