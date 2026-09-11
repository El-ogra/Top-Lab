using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByWorkGroupLog;

public sealed class GetWorkSheetByWorkGroupLogQueryHandler
    : IRequestHandler<GetWorkSheetByWorkGroupLogQuery, Result<WorkSheetDto>>
{
    private readonly IApplicationDbContext _db;

    public GetWorkSheetByWorkGroupLogQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<WorkSheetDto>> Handle(
        GetWorkSheetByWorkGroupLogQuery request, CancellationToken cancellationToken)
    {
        var (from, to) = WorkSheetPeriod.Resolve(request.From, request.To);

        var log = _db.Set<WorkGroupLog>().FirstOrDefault(l => l.Id.Value == request.WorkGroupLogId);
        if (log is null)
        {
            return Task.FromResult(Result<WorkSheetDto>.Failure(Error.NotFound("سجل مجموعة العمل غير موجود.")));
        }

        var settings = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);
        if (settings is null)
        {
            return Task.FromResult(Result<WorkSheetDto>.Failure(Error.Unexpected("سجل الإعدادات العامة مفقود.")));
        }

        var testIds = _db.Set<WorkGroupLogItem>()
            .Where(i => i.WorkGroupLogId.Equals(log.Id))
            .Select(i => i.TestId.Value)
            .ToHashSet();

        var lines = WorkSheetLines.Select(_db, testIds, from, to);
        var section = new WorkSheetSectionDto(log.Id.Value, log.Name, lines);

        return Task.FromResult(Result<WorkSheetDto>.Success(new WorkSheetDto(
            from,
            to,
            "WorkGroupLog",
            [section],
            lines.Count,
            settings.PrintFileExternalBarcode,
            settings.PrintDateTimeOnTubeBarcode,
            settings.PrintLabIdInsteadOfPatientId)));
    }
}
