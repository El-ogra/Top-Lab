using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Renders a <see cref="WorkSheetPrintEnvelope"/> visit worksheet to a PDF file
/// at the caller-supplied absolute path. Never overwrites: throws when the
/// target file already exists (defense-in-depth, mirrors the receipt writer).
/// Generation/write failures throw and are mapped by the printing service to
/// <c>Error.Unexpected</c>.
/// </summary>
public interface IWorkSheetPdfWriter
{
    Task WritePdfAsync(
        string absolutePath,
        VisitWorkSheetDto sheet,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default);
}
