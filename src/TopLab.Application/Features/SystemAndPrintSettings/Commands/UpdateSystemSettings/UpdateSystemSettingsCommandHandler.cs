using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateSystemSettings;

public sealed class UpdateSystemSettingsCommandHandler : IRequestHandler<UpdateSystemSettingsCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateSystemSettingsCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UpdateSystemSettingsCommand request, CancellationToken cancellationToken)
    {
        var row = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);
        if (row is null)
        {
            return Result.Failure(Error.Unexpected("سجل الإعدادات العامة مفقود."));
        }

        row.SetDefaultAccountType(request.DefaultAccountType);
        row.SetGeneralFlags(
            request.SaveTreatingDoctorOnlyFromEntityWindow,
            request.EnablePatientNameSearchAssist,
            request.DisableAutoTitleInsertion,
            request.PrintFileExternalBarcode,
            request.PrintDateTimeOnTubeBarcode,
            request.PrintLabIdInsteadOfPatientId,
            request.AutoReviewAndComplete,
            request.PrintAccountInsteadOfDateOnReport);
        row.SetResultScreenAccountDisplayMode(request.ResultScreenAccountDisplayMode);
        row.SetDailyBackup(request.DailyBackupEnabled, request.DailyBackupPath);

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}