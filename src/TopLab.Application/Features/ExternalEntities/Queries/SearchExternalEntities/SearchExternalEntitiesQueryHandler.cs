using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.ExternalEntities;

namespace TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;

public sealed class SearchExternalEntitiesQueryHandler
    : IRequestHandler<SearchExternalEntitiesQuery, Result<IReadOnlyList<ExternalEntityListItemDto>>>
{
    private readonly IApplicationDbContext _db;

    public SearchExternalEntitiesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<ExternalEntityListItemDto>>> Handle(
        SearchExternalEntitiesQuery request, CancellationToken cancellationToken)
    {
        var term = request.SearchTerm?.Trim();

        var query = _db.Set<ExternalEntity>().AsQueryable();
        if (request.EntityType.HasValue)
        {
            query = query.Where(e => e.EntityType == request.EntityType.Value);
        }

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(e =>
                e.Name.Contains(term)
                || (e.City != null && e.City.Contains(term))
                || (e.Phone != null && e.Phone.Contains(term))
                || (e.GeneratedIdCode != null && e.GeneratedIdCode.Contains(term)));
        }

        var rows = query
            .OrderBy(e => e.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var listIds = rows
            .Where(r => r.PriceListId != null)
            .Select(r => r.PriceListId!)
            .Distinct()
            .ToList();

        var names = _db.Set<PriceList>()
            .Where(p => listIds.Contains(p.Id))
            .ToDictionary(p => p.Id, p => p.Name);

        IReadOnlyList<ExternalEntityListItemDto> items = rows
            .Select(e => new ExternalEntityListItemDto(
                e.Id.Value,
                e.EntityType,
                e.Name,
                e.City,
                e.Phone,
                e.PriceListId == null ? null : e.PriceListId.Value,
                e.PriceListId != null && names.TryGetValue(e.PriceListId, out var priceListName) ? priceListName : null,
                e.DiscountOrCommissionPercent,
                e.GeneratedIdCode))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<ExternalEntityListItemDto>>.Success(items));
    }
}
