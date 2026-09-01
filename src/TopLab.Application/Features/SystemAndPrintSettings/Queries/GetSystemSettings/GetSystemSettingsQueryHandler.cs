using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;

public sealed class GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, Result<SystemSettingsDto>>
{
    private readonly IApplicationDbContext _db;

    public GetSystemSettingsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<SystemSettingsDto>> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken)
    {
        var row = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);

        if (row is null)
        {
            return Task.FromResult(Result<SystemSettingsDto>.Failure(Error.Unexpected("سجل الإعدادات العامة مفقود.")));
        }

        var dto = new SystemSettingsDto(
            row.DefaultAccountType,
            row.SaveTreatingDoctorOnlyFromEntityWindow,
            row.EnablePatientNameSearchAssist,
            row.DisableAutoTitleInsertion,
            row.PrintFileExternalBarcode,
            row.PrintDateTimeOnTubeBarcode,
            row.PrintLabIdInsteadOfPatientId,
            row.AutoReviewAndComplete,
            row.PrintAccountInsteadOfDateOnReport,
            row.ResultScreenAccountDisplayMode,
            row.DailyBackupEnabled,
            row.DailyBackupPath);

        return Task.FromResult(Result<SystemSettingsDto>.Success(dto));
    }
}