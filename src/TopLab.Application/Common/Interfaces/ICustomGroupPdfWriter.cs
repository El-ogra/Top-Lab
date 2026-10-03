using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// P-01 F9 (PP-03) — renders a custom test-group list to a PDF file at the
/// caller-supplied absolute path. Never overwrites.
///
/// This port is deliberately **separate from <c>IPriceListPdfWriter</c>** even though the
/// two documents have the same shape. The DTOs are unrelated types
/// (<c>CustomGroupDetailDto</c> vs <c>PriceListDetailDto</c>) and the owner listed F8 and F9
/// as two separate functions; sharing one port or one writer would force a DTO to be widened
/// to fit both (C-6, AS-6). The two print paths share no class and no DTO.
/// </summary>
public interface ICustomGroupPdfWriter
{
    Task WritePdfAsync(
        string absolutePath,
        CustomGroupDetailDto group,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default);
}