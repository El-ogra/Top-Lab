using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.Utilities.Commands.RemovePurchaseItem;

public sealed class RemovePurchaseItemCommandHandler : IRequestHandler<RemovePurchaseItemCommand, Result>
{
    private readonly IPurchasesListStore _store;

    public RemovePurchaseItemCommandHandler(IPurchasesListStore store)
    {
        _store = store;
    }

    public async Task<Result> Handle(RemovePurchaseItemCommand request, CancellationToken cancellationToken)
    {
        var load = await _store.GetAllAsync(cancellationToken);
        if (!load.IsSuccess)
        {
            return Result.Failure(load.Errors);
        }

        var remaining = load.Value!.Where(i => i.Id != request.Id).ToList();
        if (remaining.Count == load.Value!.Count)
        {
            return Result.Failure(Error.NotFound("البند غير موجود.", "NotFound"));
        }

        return await _store.SaveAllAsync(remaining, cancellationToken);
    }
}
