using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Billing;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenamePriceList;

public sealed class RenamePriceListCommandHandler : IRequestHandler<RenamePriceListCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public RenamePriceListCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(RenamePriceListCommand request, CancellationToken cancellationToken)
    {
        var list = _db.Set<PriceList>().FirstOrDefault(p => p.Id.Value == request.Id);
        if (list is null)
        {
            return Result.Failure(Error.NotFound("قائمة الأسعار غير موجودة."));
        }

        if (_db.Set<PriceList>().Any(p => p.Id.Value != request.Id && p.Name == request.Name))
        {
            return Result.Failure(Error.Conflict("قائمة أسعار موجودة بالفعل"));
        }

        try
        {
            list.Rename(request.Name);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
