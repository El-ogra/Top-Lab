using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Billing;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemovePriceListItem;

public sealed class RemovePriceListItemCommandHandler : IRequestHandler<RemovePriceListItemCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public RemovePriceListItemCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(RemovePriceListItemCommand request, CancellationToken cancellationToken)
    {
        var list = _db.Set<PriceList>().FirstOrDefault(p => p.Id.Value == request.PriceListId);
        if (list is null)
        {
            return Result.Failure(Error.NotFound("قائمة الأسعار غير موجودة."));
        }

        var existingRow = _db.Set<PriceListItem>()
            .FirstOrDefault(i => i.PriceListId.Value == request.PriceListId && i.TestId.Value == request.TestId);

        if (existingRow is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود في القائمة."));
        }

        _db.Remove(existingRow);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
