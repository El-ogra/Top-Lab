using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Billing;
using TopLab.Domain.ExternalEntities;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeletePriceList;

public sealed class DeletePriceListCommandHandler : IRequestHandler<DeletePriceListCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeletePriceListCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeletePriceListCommand request, CancellationToken cancellationToken)
    {
        var list = _db.Set<PriceList>().FirstOrDefault(p => p.Id.Value == request.Id);
        if (list is null)
        {
            return Result.Failure(Error.NotFound("قائمة الأسعار غير موجودة."));
        }

        if (_db.Set<ExternalEntity>().Any(e => e.PriceListId != null && e.PriceListId.Value == request.Id))
        {
            return Result.Failure(Error.Conflict("تعذر حذف قائمة الأسعار لارتباطها بجهات خارجية."));
        }

        _db.Remove(list);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsReferenceConflict(ex))
        {
            return Result.Failure(Error.Conflict("تعذر حذف قائمة الأسعار لارتباطها بجهات خارجية."));
        }

        return Result.Success();
    }

    private static bool IsReferenceConflict(Exception ex)
    {
        var msg = ex.Message;
        return msg.Contains("REFERENCE") || msg.Contains("conflicted");
    }
}
