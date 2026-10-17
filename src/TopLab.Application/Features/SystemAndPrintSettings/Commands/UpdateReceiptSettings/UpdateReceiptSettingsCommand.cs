using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReceiptSettings;

public sealed record UpdateReceiptSettingsCommand(
    decimal TopMarginCm,
    string Currency,
    TimeOnly? PickupTimeDefault,
    bool PrintOnce,
    TestDetailDisplayMode TestDetailDisplayMode,
    bool CashierPrinterEnabled,
    HeaderFooterMode HeaderFooterMode,
    // GAP-007: appended with defaults so every existing caller keeps compiling (SD-6).
    string? HeaderFontFamily = null,
    decimal? HeaderFontSizePt = null,
    string? HeaderFontColor = null,
    string? FooterFontFamily = null,
    decimal? FooterFontSizePt = null,
    string? FooterFontColor = null,
    string? LogoImagePath = null)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}
