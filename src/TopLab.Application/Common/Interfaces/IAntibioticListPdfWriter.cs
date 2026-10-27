using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// GAP-25 (3.9.1.6.5) — renders the antibiotic catalogue to a PDF file at the
/// caller-supplied absolute path. Never overwrites: throws when the target file
/// already exists (mirrors <see cref="IPriceListPdfWriter"/>).
///
/// The port lives in Application so Presentation depends only on the abstraction
/// and never names an Infrastructure type (SD-10, ADR-0005). It is deliberately
/// specific to the antibiotic catalogue and is not a generic list writer.
/// </summary>
public interface IAntibioticListPdfWriter
{
    Task WritePdfAsync(
        string absolutePath,
        IReadOnlyList<AntibioticDto> antibiotics,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default);
}
