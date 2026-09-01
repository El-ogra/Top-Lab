using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateSystemSettings;

public sealed record UpdateSystemSettingsCommand(
    AccountType DefaultAccountType,
    bool SaveTreatingDoctorOnlyFromEntityWindow,
    bool EnablePatientNameSearchAssist,
    bool DisableAutoTitleInsertion,
    bool PrintFileExternalBarcode,
    bool PrintDateTimeOnTubeBarcode,
    bool PrintLabIdInsteadOfPatientId,
    bool AutoReviewAndComplete,
    bool PrintAccountInsteadOfDateOnReport,
    ResultScreenAccountDisplayMode ResultScreenAccountDisplayMode,
    bool DailyBackupEnabled,
    string? DailyBackupPath)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}