using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroups;

public sealed class GetCustomGroupsQueryHandler : IRequestHandler<GetCustomGroupsQuery, Result<IReadOnlyList<CustomGroupSummaryDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetCustomGroupsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<CustomGroupSummaryDto>>> Handle(GetCustomGroupsQuery request, CancellationToken cancellationToken)
    {
        var itemCounts = _db.Set<CustomGroupItem>()
            .GroupBy(i => i.CustomGroupId.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var groups = _db.Set<CustomGroup>().OrderBy(g => g.Name).ToList();

        var dtos = groups
            .Select(g => new CustomGroupSummaryDto(
                g.Id.Value,
                g.Name,
                itemCounts.TryGetValue(g.Id.Value, out var count) ? count : 0))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<CustomGroupSummaryDto>>.Success(dtos));
    }
}
