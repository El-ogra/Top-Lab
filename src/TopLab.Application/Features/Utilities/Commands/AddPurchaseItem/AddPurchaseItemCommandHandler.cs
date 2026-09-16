using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Features.Utilities.Commands.AddPurchaseItem;

public sealed class AddPurchaseItemCommandHandler : IRequestHandler<AddPurchaseItemCommand, Result>
{
    private readonly IPurchasesListStore _store;
    private readonly IDateTimeProvider _dateTime;

    public AddPurchaseItemCommandHandler(IPurchasesListStore store, IDateTimeProvider dateTime)
    {
        _store = store;
        _dateTime = dateTime;
    }

    public async Task<Result> Handle(AddPurchaseItemCommand request, CancellationToken cancellationToken)
    {
        var load = await _store.GetAllAsync(cancellationToken);
        if (!load.IsSuccess)
        {
            return Result.Failure(load.Errors);
        }

        var items = load.Value!.ToList();
        var nextId = items.Count == 0 ? 1 : items.Max(i => i.Id) + 1;
        items.Add(new PurchaseItemDto(nextId, request.Text.Trim(), false, _dateTime.UtcNow));

        return await _store.SaveAllAsync(items, cancellationToken);
    }
}
