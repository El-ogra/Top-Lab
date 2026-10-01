using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportSettings;

public sealed record UpdateReportSettingsCommand(
    decimal PageMarginLeftCm,
    decimal PageMarginBottomCm,
    decimal ReportTopSpaceCm,
    PaperSize PaperSize,
    HeaderFooterMode HeaderFooterMode,
    bool DoctorSignatureEnabled,
    HistorySortMode HistorySortMode,
    bool HistoryAutoDisplayEnabled,
    // W-02 S7 (WP-13): appended with defaults so every existing caller keeps compiling (SD-6).
    bool PrintGroupSubTitle = false,
    bool SuppressReprintMessage = false)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}