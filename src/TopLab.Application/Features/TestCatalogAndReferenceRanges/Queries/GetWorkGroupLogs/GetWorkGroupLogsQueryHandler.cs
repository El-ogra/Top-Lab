using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetWorkGroupLogs;

public sealed class GetWorkGroupLogsQueryHandler : IRequestHandler<GetWorkGroupLogsQuery, Result<IReadOnlyList<WorkGroupLogDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetWorkGroupLogsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<WorkGroupLogDto>>> Handle(GetWorkGroupLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = _db.Set<WorkGroupLog>().OrderBy(l => l.Name).ToList();
        var itemsByLog = _db.Set<WorkGroupLogItem>()
            .GroupBy(i => i.WorkGroupLogId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var testNameById = _db.Set<Test>().ToDictionary(t => t.Id, t => t.Name);

        var dtos = logs
            .Select(l =>
            {
                var names = itemsByLog.TryGetValue(l.Id, out var logItems)
                    ? logItems.Select(i => new WorkGroupLogItemDto(i.TestId.Value, testNameById.GetValueOrDefault(i.TestId) ?? string.Empty)).ToList()
                    : new List<WorkGroupLogItemDto>();
                return new WorkGroupLogDto(l.Id.Value, l.Name, names);
            })
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<WorkGroupLogDto>>.Success(dtos));
    }
}