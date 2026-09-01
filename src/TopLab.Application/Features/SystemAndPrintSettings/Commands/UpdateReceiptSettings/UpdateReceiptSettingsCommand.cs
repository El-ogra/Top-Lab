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
    HeaderFooterMode HeaderFooterMode)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}