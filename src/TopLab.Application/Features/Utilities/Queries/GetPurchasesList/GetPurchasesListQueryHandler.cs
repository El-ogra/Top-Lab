using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Features.Utilities.Queries.GetPurchasesList;

public sealed class GetPurchasesListQueryHandler
    : IRequestHandler<GetPurchasesListQuery, Result<IReadOnlyList<PurchaseItemDto>>>
{
    private readonly IPurchasesListStore _store;

    public GetPurchasesListQueryHandler(IPurchasesListStore store)
    {
        _store = store;
    }

    public async Task<Result<IReadOnlyList<PurchaseItemDto>>> Handle(
        GetPurchasesListQuery request,
        CancellationToken cancellationToken)
    {
        var load = await _store.GetAllAsync(cancellationToken);
        if (!load.IsSuccess)
        {
            return Result<IReadOnlyList<PurchaseItemDto>>.Failure(load.Errors);
        }

        IReadOnlyList<PurchaseItemDto> ordered = load.Value!
            .OrderBy(i => i.Id)
            .ToList();
        return Result<IReadOnlyList<PurchaseItemDto>>.Success(ordered);
    }
}
