using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetSummary;

public sealed class GetWorkSheetSummaryQueryHandler
    : IRequestHandler<GetWorkSheetSummaryQuery, Result<IReadOnlyList<WorkSheetSummaryRowDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetWorkSheetSummaryQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<WorkSheetSummaryRowDto>>> Handle(
        GetWorkSheetSummaryQuery request, CancellationToken cancellationToken)
    {
        var (from, to) = WorkSheetPeriod.Resolve(request.From, request.To);

        var logs = _db.Set<WorkGroupLog>().OrderBy(l => l.Name).ToList();

        IReadOnlyList<WorkSheetSummaryRowDto> rows = logs
            .Select(log =>
            {
                var testIds = _db.Set<WorkGroupLogItem>()
                    .Where(i => i.WorkGroupLogId.Equals(log.Id))
                    .Select(i => i.TestId.Value)
                    .ToHashSet();

                var lines = WorkSheetLines.Select(_db, testIds, from, to);

                return new WorkSheetSummaryRowDto(
                    log.Id.Value,
                    log.Name,
                    lines.Count(l => !l.IsSampleDrawn),
                    lines.Count(l => l.IsSampleDrawn && !l.HasResult),
                    lines.Count(l => l.HasResult));
            })
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<WorkSheetSummaryRowDto>>.Success(rows));
    }
}
