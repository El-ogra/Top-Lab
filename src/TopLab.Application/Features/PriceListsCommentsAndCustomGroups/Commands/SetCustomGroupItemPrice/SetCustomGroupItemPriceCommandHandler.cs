using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetCustomGroupItemPrice;

public sealed class SetCustomGroupItemPriceCommandHandler : IRequestHandler<SetCustomGroupItemPriceCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public SetCustomGroupItemPriceCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(SetCustomGroupItemPriceCommand request, CancellationToken cancellationToken)
    {
        var group = _db.Set<CustomGroup>().FirstOrDefault(g => g.Id.Value == request.CustomGroupId);
        if (group is null)
        {
            return Result.Failure(Error.NotFound("المجموعة غير موجودة."));
        }

        if (!_db.Set<Test>().Any(t => t.Id.Value == request.TestId))
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        var existingRow = _db.Set<CustomGroupItem>()
            .FirstOrDefault(i => i.CustomGroupId.Value == request.CustomGroupId && i.TestId.Value == request.TestId);

        try
        {
            group.SetItemPrice(TestId.Create(request.TestId), request.Price);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        if (existingRow is null)
        {
            _db.Add(new CustomGroupItem(
                CustomGroupId.Create(request.CustomGroupId),
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
