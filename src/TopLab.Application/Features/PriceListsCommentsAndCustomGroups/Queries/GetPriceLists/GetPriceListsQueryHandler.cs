using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Billing;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceLists;

public sealed class GetPriceListsQueryHandler : IRequestHandler<GetPriceListsQuery, Result<IReadOnlyList<PriceListSummaryDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetPriceListsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<PriceListSummaryDto>>> Handle(GetPriceListsQuery request, CancellationToken cancellationToken)
    {
        var itemCounts = _db.Set<PriceListItem>()
            .GroupBy(i => i.PriceListId.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var lists = _db.Set<PriceList>().OrderBy(l => l.Name).ToList();

        var dtos = lists
            .Select(l => new PriceListSummaryDto(
                l.Id.Value,
                l.Name,
                itemCounts.TryGetValue(l.Id.Value, out var count) ? count : 0))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<PriceListSummaryDto>>.Success(dtos));
    }
}
