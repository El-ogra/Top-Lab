using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// P-01 F8 (PP-03) — renders a price list to a PDF file at the caller-supplied
/// absolute path. Never overwrites: throws when the target file already exists
/// (mirrors <see cref="IWorkSheetPdfWriter"/>).
///
/// The port lives in Application so Presentation depends only on the abstraction
/// and never names an Infrastructure type (SD-10, ADR-0005).
///
/// This port is deliberately **specific to price lists**. It is not a generic
/// "list writer": the custom test-group list (F9) has its own port,
/// <c>ICustomGroupPdfWriter</c>, over an unrelated DTO (C-6, AS-6).
/// </summary>
public interface IPriceListPdfWriter
{
    Task WritePdfAsync(
        string absolutePath,
        PriceListDetailDto priceList,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default);
}