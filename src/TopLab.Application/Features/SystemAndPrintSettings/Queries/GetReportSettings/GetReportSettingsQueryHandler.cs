using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReportSettings;

public sealed class GetReportSettingsQueryHandler : IRequestHandler<GetReportSettingsQuery, Result<ReportSettingsDto>>
{
    private readonly IApplicationDbContext _db;

    public GetReportSettingsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<ReportSettingsDto>> Handle(GetReportSettingsQuery request, CancellationToken cancellationToken)
    {
        var row = _db.Set<ReportSettings>().SingleOrDefault(s => s.Id == 1);

        if (row is null)
        {
            return Task.FromResult(Result<ReportSettingsDto>.Failure(Error.Unexpected("سجل إعدادات التقرير مفقود.")));
        }

        var dto = new ReportSettingsDto(
            row.PageMarginLeftCm,
            row.PageMarginBottomCm,
            row.ReportTopSpaceCm,
            row.PaperSize,
            row.HeaderFooterMode,
            row.DoctorSignatureEnabled,
            row.HistorySortMode,
            row.HistoryAutoDisplayEnabled);

        return Task.FromResult(Result<ReportSettingsDto>.Success(dto));
    }
}