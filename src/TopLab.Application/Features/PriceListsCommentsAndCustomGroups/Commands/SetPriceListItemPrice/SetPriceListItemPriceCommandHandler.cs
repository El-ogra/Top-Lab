using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetPriceListItemPrice;

public sealed class SetPriceListItemPriceCommandHandler : IRequestHandler<SetPriceListItemPriceCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SetPriceListItemPriceCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(SetPriceListItemPriceCommand request, CancellationToken cancellationToken)
    {
        var list = _db.Set<PriceList>().FirstOrDefault(p => p.Id.Value == request.PriceListId);
        if (list is null)
        {
            return Result.Failure(Error.NotFound("قائمة الأسعار غير موجودة."));
        }

        if (!_db.Set<Test>().Any(t => t.Id.Value == request.TestId))
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        var existingRow = _db.Set<PriceListItem>()
            .FirstOrDefault(i => i.PriceListId.Value == request.PriceListId && i.TestId.Value == request.TestId);

        try
        {
            list.SetItemPrice(TestId.Create(request.TestId), request.Price);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        if (existingRow is null)
        {
            _db.Add(new PriceListItem(
                PriceListId.Create(request.PriceListId),
                TestId.Create(request.TestId),
                request.Price));
        }
        else
        {
            existingRow.UpdatePrice(request.Price);
            _db.Update(existingRow);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
