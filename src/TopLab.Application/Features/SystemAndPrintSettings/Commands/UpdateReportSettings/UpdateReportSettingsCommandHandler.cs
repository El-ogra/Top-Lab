using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportSettings;

public sealed class UpdateReportSettingsCommandHandler : IRequestHandler<UpdateReportSettingsCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateReportSettingsCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UpdateReportSettingsCommand request, CancellationToken cancellationToken)
    {
        var row = _db.Set<ReportSettings>().SingleOrDefault(s => s.Id == 1);
        if (row is null)
        {
            return Result.Failure(Error.Unexpected("سجل إعدادات التقرير مفقود."));
        }

        row.SetMargins(request.PageMarginLeftCm, request.PageMarginBottomCm);
        row.SetTopSpace(request.ReportTopSpaceCm);
        row.SetPaperSize(request.PaperSize);
        row.SetHeaderFooterMode(request.HeaderFooterMode);
        row.SetDoctorSignature(request.DoctorSignatureEnabled);
        row.SetHistoryOptions(request.HistorySortMode, request.HistoryAutoDisplayEnabled);

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}