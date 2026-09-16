using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Utilities.Common;

namespace TopLab.Application.Features.Utilities.Commands.TogglePurchaseItemDone;

public sealed class TogglePurchaseItemDoneCommandHandler : IRequestHandler<TogglePurchaseItemDoneCommand, Result>
{
    private readonly IPurchasesListStore _store;

    public TogglePurchaseItemDoneCommandHandler(IPurchasesListStore store)
    {
        _store = store;
    }

    public async Task<Result> Handle(TogglePurchaseItemDoneCommand request, CancellationToken cancellationToken)
    {
        var load = await _store.GetAllAsync(cancellationToken);
        if (!load.IsSuccess)
        {
            return Result.Failure(load.Errors);
        }

        var found = false;
        var updated = load.Value!.Select(i =>
        {
            if (i.Id != request.Id)
            {
                return i;
            }

            found = true;
            return i with { IsDone = !i.IsDone };
        }).ToList();

        if (!found)
        {
            return Result.Failure(Error.NotFound("البند غير موجود.", "NotFound"));
        }

        return await _store.SaveAllAsync(updated, cancellationToken);
    }
}
