using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetPriceListById;

public sealed class GetPriceListByIdQueryHandler : IRequestHandler<GetPriceListByIdQuery, Result<PriceListDetailDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPriceListByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<PriceListDetailDto>> Handle(GetPriceListByIdQuery request, CancellationToken cancellationToken)
    {
        var list = _db.Set<PriceList>().FirstOrDefault(l => l.Id.Value == request.Id);
        if (list is null)
        {
            return Task.FromResult(Result<PriceListDetailDto>.Failure(Error.NotFound("قائمة الأسعار غير موجودة.")));
        }

        var testNamesById = _db.Set<Test>().ToDictionary(t => t.Id, t => (t.Name, t.TestCode));

        var items = _db.Set<PriceListItem>()
            .Where(i => i.PriceListId.Value == request.Id)
            .ToList()
            .Select(i =>
            {
                testNamesById.TryGetValue(i.TestId, out var info);
                return new PriceListItemDto(
                    i.TestId.Value,
                    info.Name,
                    info.TestCode,
                    i.Price);
            })
            .ToList();

        var dto = new PriceListDetailDto(list.Id.Value, list.Name, items);

        return Task.FromResult(Result<PriceListDetailDto>.Success(dto));
    }
}
